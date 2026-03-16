using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Infrastructure.Services;

public sealed class UploadService(
    TaxTrackDbContext dbContext,
    ICompanyAccessService companyAccessService,
    IAuditService auditService,
    ILogger<UploadService> logger) : IUploadService
{
    public async Task<UploadAcceptedResponse> UploadAsync(
        UploadCommand command,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var canAccess = await companyAccessService.CanAccessCompanyAsync(command.UserId, command.CompanyId, cancellationToken);
        if (!canAccess)
        {
            throw new ForbiddenException("User cannot upload data for this company.");
        }

        var company = await dbContext.Companies.FirstOrDefaultAsync(x => x.Id == command.CompanyId, cancellationToken);
        if (company is null)
        {
            throw new KeyNotFoundException("Company not found.");
        }

        using var copyStream = new MemoryStream();
        await command.Content.CopyToAsync(copyStream, cancellationToken);
        var bytes = copyStream.ToArray();
        var payloadHash = ComputePayloadHash(command.CompanyId, command.DatasetType, bytes);

        var existingIdempotency = await dbContext.IdempotencyRecords.FirstOrDefaultAsync(
            x => x.UserId == command.UserId &&
                 x.Endpoint == "POST:/api/financial/upload" &&
                 x.IdempotencyKey == command.IdempotencyKey,
            cancellationToken);

        if (existingIdempotency is not null)
        {
            if (!string.Equals(existingIdempotency.RequestHash, payloadHash, StringComparison.Ordinal))
            {
                throw new ConflictException("Idempotency key already used with a different payload.");
            }

            var existingJob = await dbContext.UploadJobs.FirstAsync(x => x.Id == existingIdempotency.ResourceId, cancellationToken);
            return new UploadAcceptedResponse(existingJob.Id, existingJob.CompanyId, existingJob.DatasetType, existingJob.Status, existingJob.CreatedAtUtc);
        }

        var uploadJob = new UploadJob
        {
            CompanyId = command.CompanyId,
            RequestedByUserId = command.UserId,
            DatasetType = command.DatasetType,
            Status = UploadJobStatus.Validating
        };
        var idempotencyRecord = new IdempotencyRecord
        {
            UserId = command.UserId,
            Endpoint = "POST:/api/financial/upload",
            IdempotencyKey = command.IdempotencyKey,
            RequestHash = payloadHash,
            ResourceId = uploadJob.Id
        };

        var issues = new List<ValidationIssue>();
        ParseSummary summary;
        List<FinancialTransaction>? transactionRecords = null;
        List<PayrollRecord>? payrollRecords = null;
        List<VatSubmissionRecord>? vatRecords = null;

        try
        {
            switch (command.DatasetType)
            {
                case DatasetType.Transactions:
                {
                    var existingIds = await dbContext.FinancialTransactions
                        .Where(x => x.CompanyId == command.CompanyId)
                        .Select(x => x.SourceRecordId)
                        .ToListAsync(cancellationToken);
                    var result = ParseTransactions(
                        command.CompanyId,
                        company.RegistrationNumber,
                        uploadJob.Id,
                        bytes,
                        new HashSet<string>(existingIds, StringComparer.OrdinalIgnoreCase),
                        issues);
                    summary = result.Summary;
                    transactionRecords = result.Records;
                    break;
                }
                case DatasetType.Payroll:
                {
                    var existingIds = await dbContext.PayrollRecords
                        .Where(x => x.CompanyId == command.CompanyId)
                        .Select(x => x.SourceRecordId)
                        .ToListAsync(cancellationToken);
                    var result = ParsePayroll(
                        command.CompanyId,
                        company.RegistrationNumber,
                        uploadJob.Id,
                        bytes,
                        new HashSet<string>(existingIds, StringComparer.OrdinalIgnoreCase),
                        issues);
                    summary = result.Summary;
                    payrollRecords = result.Records;
                    break;
                }
                case DatasetType.VatSubmissions:
                {
                    var existingIds = await dbContext.VatSubmissionRecords
                        .Where(x => x.CompanyId == command.CompanyId)
                        .Select(x => x.SourceRecordId)
                        .ToListAsync(cancellationToken);
                    var result = ParseVatSubmissions(
                        command.CompanyId,
                        company.RegistrationNumber,
                        uploadJob.Id,
                        bytes,
                        new HashSet<string>(existingIds, StringComparer.OrdinalIgnoreCase),
                        issues);
                    summary = result.Summary;
                    vatRecords = result.Records;
                    break;
                }
                default:
                    throw new TaxTrack.Application.Exceptions.ValidationException(
                        "Unsupported dataset type.",
                        [new ValidationIssue(1, "datasetType", "DC005", "Unsupported dataset type.")]);
            }
        }
        catch (HeaderValidationException ex)
        {
            issues.Add(new ValidationIssue(1, "header", "DC001", $"Missing or invalid required columns. {ex.Message}"));
            summary = new ParseSummary(0, 0);
        }
        catch (ReaderException ex)
        {
            issues.Add(new ValidationIssue(1, "file", "DC010", $"CSV parsing failure. {ex.Message}"));
            summary = new ParseSummary(0, 0);
        }

        uploadJob.AcceptedRows = summary.AcceptedRows;
        uploadJob.RejectedRows = issues.Count;
        uploadJob.EvidenceCompleteness = summary.EvidenceCompleteness;
        uploadJob.InsufficientEvidence = summary.EvidenceCompleteness < 80;
        uploadJob.UpdatedAtUtc = DateTime.UtcNow;

        if (issues.Count > 0)
        {
            uploadJob.Status = UploadJobStatus.Failed;
            uploadJob.ValidationErrorsJson = JsonSerializer.Serialize(issues.Take(1000).ToArray());
        }
        else
        {
            if (transactionRecords is not null)
            {
                dbContext.FinancialTransactions.AddRange(transactionRecords);
            }

            if (payrollRecords is not null)
            {
                dbContext.PayrollRecords.AddRange(payrollRecords);
            }

            if (vatRecords is not null)
            {
                dbContext.VatSubmissionRecords.AddRange(vatRecords);
            }

            uploadJob.Status = UploadJobStatus.Completed;
        }

        dbContext.UploadJobs.Add(uploadJob);
        dbContext.IdempotencyRecords.Add(idempotencyRecord);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (issues.Count > 0)
        {
            logger.LogWarning(
                "Upload validation failed {UploadId} {CompanyId} {DatasetType} {IssueCount} {CorrelationId}",
                uploadJob.Id,
                command.CompanyId,
                command.DatasetType,
                issues.Count,
                correlationId);

            await auditService.LogAsync(
                command.UserId,
                command.CompanyId,
                AuditEventType.UploadValidationFailed,
                correlationId,
                new { uploadJob.Id, uploadJob.DatasetType, issueCount = issues.Count },
                ipAddress,
                userAgent,
                cancellationToken);

            throw new TaxTrack.Application.Exceptions.ValidationException("CSV validation failed.", issues.Take(1000).ToArray());
        }

        logger.LogInformation(
            "Upload completed {UploadId} {CompanyId} {DatasetType} {AcceptedRows} {CorrelationId}",
            uploadJob.Id,
            command.CompanyId,
            command.DatasetType,
            uploadJob.AcceptedRows,
            correlationId);

        await auditService.LogAsync(
            command.UserId,
            command.CompanyId,
            AuditEventType.UploadCreated,
            correlationId,
            new { uploadJob.Id, uploadJob.DatasetType, uploadJob.AcceptedRows },
            ipAddress,
            userAgent,
            cancellationToken);

        return new UploadAcceptedResponse(uploadJob.Id, uploadJob.CompanyId, uploadJob.DatasetType, uploadJob.Status, uploadJob.CreatedAtUtc);
    }

    public async Task<UploadStatusResponse?> GetUploadStatusAsync(Guid userId, Guid uploadId, CancellationToken cancellationToken)
    {
        var uploadJob = await dbContext.UploadJobs.FirstOrDefaultAsync(x => x.Id == uploadId, cancellationToken);
        if (uploadJob is null)
        {
            return null;
        }

        var canAccess = await companyAccessService.CanAccessCompanyAsync(userId, uploadJob.CompanyId, cancellationToken);
        if (!canAccess)
        {
            throw new ForbiddenException("User cannot access this upload.");
        }

        return new UploadStatusResponse(
            uploadJob.Id,
            uploadJob.CompanyId,
            uploadJob.Status,
            uploadJob.AcceptedRows,
            uploadJob.RejectedRows,
            uploadJob.EvidenceCompleteness,
            uploadJob.InsufficientEvidence,
            uploadJob.UpdatedAtUtc);
    }

    private ParseResult<FinancialTransaction> ParseTransactions(
        Guid companyId,
        string expectedRegistration,
        Guid uploadJobId,
        byte[] bytes,
        ISet<string> existingSourceIds,
        ICollection<ValidationIssue> issues)
    {
        var accepted = 0;
        var evidenceRequired = 0;
        var evidencePresent = 0;
        var sourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var records = new List<FinancialTransaction>();

        using var stream = new MemoryStream(bytes);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var row = csv.Parser.Row;
            var contractVersion = csv.GetField("contract_version");
            var sourceRecordId = csv.GetField("source_record_id") ?? string.Empty;
            var registration = csv.GetField("company_registration_number") ?? string.Empty;
            var transactionDate = csv.GetField("transaction_date");
            var ledgerCategory = csv.GetField("ledger_category") ?? string.Empty;
            var description = csv.GetField("description") ?? string.Empty;
            var amountText = csv.GetField("amount");
            var currency = csv.GetField("currency");
            var vatAmountText = csv.GetField("vat_amount");
            var direction = csv.GetField("direction") ?? string.Empty;
            var sourceSystem = csv.GetField("source_system") ?? string.Empty;
            var taxInvoiceNumber = csv.GetField("tax_invoice_number");
            var supplierVatNumber = csv.GetField("supplier_vat_number");
            var taxInvoiceDateText = csv.GetField("tax_invoice_date");
            var vat201Reference = csv.GetField("vat201_reference");

            if (!string.Equals(contractVersion, "v1", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ValidationIssue(row, "contract_version", "DC002", "Unsupported contract version."));
            }

            if (string.IsNullOrWhiteSpace(sourceRecordId))
            {
                issues.Add(new ValidationIssue(row, "source_record_id", "DC006", "source_record_id is required."));
            }
            else if (!sourceIds.Add(sourceRecordId))
            {
                issues.Add(new ValidationIssue(row, "source_record_id", "DC008", "Duplicate source_record_id in file."));
            }
            else if (existingSourceIds.Contains(sourceRecordId))
            {
                issues.Add(new ValidationIssue(row, "source_record_id", "DC014", "Duplicate source_record_id already exists for this company."));
            }

            if (!string.Equals(registration, expectedRegistration, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ValidationIssue(row, "company_registration_number", "DC009", "Unknown or mismatched company registration number."));
            }

            if (!DateOnly.TryParseExact(transactionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                issues.Add(new ValidationIssue(row, "transaction_date", "DC003", "Invalid ISO date format."));
            }

            if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
            {
                issues.Add(new ValidationIssue(row, "amount", "DC004", "Invalid decimal amount."));
            }

            if (!decimal.TryParse(vatAmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var vatAmount) || vatAmount < 0)
            {
                issues.Add(new ValidationIssue(row, "vat_amount", "DC004", "Invalid VAT decimal amount."));
            }

            if (!string.Equals(direction, "debit", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(direction, "credit", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ValidationIssue(row, "direction", "DC005", "direction must be debit or credit."));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                issues.Add(new ValidationIssue(row, "description", "DC006", "description is required."));
            }

            var requiresEvidence = string.Equals(ledgerCategory, "VATInput", StringComparison.OrdinalIgnoreCase);
            if (requiresEvidence)
            {
                evidenceRequired += 3;
                if (!string.IsNullOrWhiteSpace(taxInvoiceNumber)) evidencePresent++;
                if (!string.IsNullOrWhiteSpace(supplierVatNumber)) evidencePresent++;
                if (DateOnly.TryParseExact(taxInvoiceDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    evidencePresent++;
                }
            }

            if (requiresEvidence && string.IsNullOrWhiteSpace(taxInvoiceNumber))
            {
                issues.Add(new ValidationIssue(row, "tax_invoice_number", "DC011", "Missing statutory evidence field."));
            }

            if (requiresEvidence && string.IsNullOrWhiteSpace(supplierVatNumber))
            {
                issues.Add(new ValidationIssue(row, "supplier_vat_number", "DC011", "Missing statutory evidence field."));
            }

            if (requiresEvidence && !DateOnly.TryParseExact(taxInvoiceDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                issues.Add(new ValidationIssue(row, "tax_invoice_date", "DC011", "Missing or invalid statutory evidence field."));
            }

            if (issues.Any(x => x.RowNumber == row))
            {
                continue;
            }

            records.Add(new FinancialTransaction
            {
                CompanyId = companyId,
                UploadJobId = uploadJobId,
                SourceRecordId = sourceRecordId,
                TransactionDate = parsedDate,
                LedgerCategory = ledgerCategory,
                Description = description,
                Amount = amount,
                Currency = string.IsNullOrWhiteSpace(currency) ? "ZAR" : currency,
                VatAmount = vatAmount,
                Direction = direction,
                SourceSystem = sourceSystem,
                TaxInvoiceNumber = string.IsNullOrWhiteSpace(taxInvoiceNumber) ? null : taxInvoiceNumber,
                SupplierVatNumber = string.IsNullOrWhiteSpace(supplierVatNumber) ? null : supplierVatNumber,
                TaxInvoiceDate = DateOnly.TryParseExact(taxInvoiceDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var invoiceDate) ? invoiceDate : null,
                Vat201Reference = string.IsNullOrWhiteSpace(vat201Reference) ? null : vat201Reference
            });
            accepted++;
        }

        var completeness = evidenceRequired == 0 ? 100 : (int)Math.Round((double)evidencePresent * 100 / evidenceRequired);
        return new ParseResult<FinancialTransaction>(records, new ParseSummary(accepted, completeness));
    }

    private ParseResult<PayrollRecord> ParsePayroll(
        Guid companyId,
        string expectedRegistration,
        Guid uploadJobId,
        byte[] bytes,
        ISet<string> existingSourceIds,
        ICollection<ValidationIssue> issues)
    {
        var accepted = 0;
        var evidenceRequired = 0;
        var evidencePresent = 0;
        var sourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var records = new List<PayrollRecord>();

        using var stream = new MemoryStream(bytes);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var row = csv.Parser.Row;
            var contractVersion = csv.GetField("contract_version");
            var sourceRecordId = csv.GetField("source_record_id") ?? string.Empty;
            var registration = csv.GetField("company_registration_number") ?? string.Empty;
            var payPeriodText = csv.GetField("pay_period");
            var grossSalaryText = csv.GetField("gross_salary");
            var totalPayeText = csv.GetField("total_paye");
            var employeeCountText = csv.GetField("employee_count");
            var totalUifText = csv.GetField("total_uif");
            var totalSdlText = csv.GetField("total_sdl");
            var currency = csv.GetField("currency");
            var emp201PeriodText = csv.GetField("emp201_period");
            var emp201DeclaredPayeText = csv.GetField("emp201_declared_paye");
            var emp201DeclaredUifText = csv.GetField("emp201_declared_uif");
            var emp201DeclaredSdlText = csv.GetField("emp201_declared_sdl");
            var emp501Reference = csv.GetField("emp501_reference") ?? string.Empty;
            var irp5TotalPayeText = csv.GetField("irp5_total_paye");
            var submissionDateText = csv.GetField("submission_date");
            var paymentDateText = csv.GetField("payment_date");
            var paymentReference = csv.GetField("payment_reference") ?? string.Empty;

            if (!string.Equals(contractVersion, "v1", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ValidationIssue(row, "contract_version", "DC002", "Unsupported contract version."));
            }

            if (string.IsNullOrWhiteSpace(sourceRecordId))
            {
                issues.Add(new ValidationIssue(row, "source_record_id", "DC006", "source_record_id is required."));
            }
            else if (!sourceIds.Add(sourceRecordId))
            {
                issues.Add(new ValidationIssue(row, "source_record_id", "DC008", "Duplicate source_record_id in file."));
            }
            else if (existingSourceIds.Contains(sourceRecordId))
            {
                issues.Add(new ValidationIssue(row, "source_record_id", "DC014", "Duplicate source_record_id already exists for this company."));
            }

            if (!string.Equals(registration, expectedRegistration, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ValidationIssue(row, "company_registration_number", "DC009", "Unknown or mismatched company registration number."));
            }

            DateOnly payPeriod = default;
            DateOnly emp201Period = default;
            DateOnly submissionDate = default;
            DateOnly paymentDate = default;
            decimal grossSalary = default;
            decimal totalPaye = default;
            int employeeCount = default;
            decimal totalUif = default;
            decimal totalSdl = default;
            decimal emp201DeclaredPaye = default;
            decimal emp201DeclaredUif = default;
            decimal emp201DeclaredSdl = default;
            decimal irp5TotalPaye = default;

            var parseOk =
                DateOnly.TryParseExact(payPeriodText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out payPeriod) &&
                DateOnly.TryParseExact(emp201PeriodText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out emp201Period) &&
                DateOnly.TryParseExact(submissionDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out submissionDate) &&
                DateOnly.TryParseExact(paymentDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out paymentDate) &&
                decimal.TryParse(grossSalaryText, NumberStyles.Number, CultureInfo.InvariantCulture, out grossSalary) &&
                decimal.TryParse(totalPayeText, NumberStyles.Number, CultureInfo.InvariantCulture, out totalPaye) &&
                int.TryParse(employeeCountText, NumberStyles.Integer, CultureInfo.InvariantCulture, out employeeCount) &&
                decimal.TryParse(totalUifText, NumberStyles.Number, CultureInfo.InvariantCulture, out totalUif) &&
                decimal.TryParse(totalSdlText, NumberStyles.Number, CultureInfo.InvariantCulture, out totalSdl) &&
                decimal.TryParse(emp201DeclaredPayeText, NumberStyles.Number, CultureInfo.InvariantCulture, out emp201DeclaredPaye) &&
                decimal.TryParse(emp201DeclaredUifText, NumberStyles.Number, CultureInfo.InvariantCulture, out emp201DeclaredUif) &&
                decimal.TryParse(emp201DeclaredSdlText, NumberStyles.Number, CultureInfo.InvariantCulture, out emp201DeclaredSdl) &&
                decimal.TryParse(irp5TotalPayeText, NumberStyles.Number, CultureInfo.InvariantCulture, out irp5TotalPaye);

            if (!parseOk)
            {
                issues.Add(new ValidationIssue(row, "row", "DC004", "Invalid payroll row format."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(emp501Reference))
            {
                issues.Add(new ValidationIssue(row, "emp501_reference", "DC011", "Missing statutory evidence field."));
            }

            if (string.IsNullOrWhiteSpace(paymentReference))
            {
                issues.Add(new ValidationIssue(row, "payment_reference", "DC011", "Missing statutory evidence field."));
            }

            evidenceRequired += 8;
            if (!string.IsNullOrWhiteSpace(emp501Reference)) evidencePresent++;
            if (!string.IsNullOrWhiteSpace(paymentReference)) evidencePresent++;
            if (DateOnly.TryParseExact(emp201PeriodText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) evidencePresent++;
            if (decimal.TryParse(emp201DeclaredPayeText, NumberStyles.Number, CultureInfo.InvariantCulture, out _)) evidencePresent++;
            if (decimal.TryParse(emp201DeclaredUifText, NumberStyles.Number, CultureInfo.InvariantCulture, out _)) evidencePresent++;
            if (decimal.TryParse(emp201DeclaredSdlText, NumberStyles.Number, CultureInfo.InvariantCulture, out _)) evidencePresent++;
            if (decimal.TryParse(irp5TotalPayeText, NumberStyles.Number, CultureInfo.InvariantCulture, out _)) evidencePresent++;
            if (DateOnly.TryParseExact(submissionDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) evidencePresent++;

            if (issues.Any(x => x.RowNumber == row))
            {
                continue;
            }

            if (paymentDate < payPeriod)
            {
                issues.Add(new ValidationIssue(row, "payment_date", "DC013", "payment_date cannot be before pay_period."));
                continue;
            }

            records.Add(new PayrollRecord
            {
                CompanyId = companyId,
                UploadJobId = uploadJobId,
                SourceRecordId = sourceRecordId,
                PayPeriod = payPeriod,
                GrossSalary = grossSalary,
                TotalPaye = totalPaye,
                EmployeeCount = employeeCount,
                TotalUif = totalUif,
                TotalSdl = totalSdl,
                Currency = string.IsNullOrWhiteSpace(currency) ? "ZAR" : currency!,
                Emp201Period = emp201Period,
                Emp201DeclaredPaye = emp201DeclaredPaye,
                Emp201DeclaredUif = emp201DeclaredUif,
                Emp201DeclaredSdl = emp201DeclaredSdl,
                Emp501Reference = emp501Reference,
                Irp5TotalPaye = irp5TotalPaye,
                SubmissionDate = submissionDate,
                PaymentDate = paymentDate,
                PaymentReference = paymentReference
            });
            accepted++;
        }

        var completeness = evidenceRequired == 0 ? 100 : (int)Math.Round((double)evidencePresent * 100 / evidenceRequired);
        return new ParseResult<PayrollRecord>(records, new ParseSummary(accepted, completeness));
    }

    private ParseResult<VatSubmissionRecord> ParseVatSubmissions(
        Guid companyId,
        string expectedRegistration,
        Guid uploadJobId,
        byte[] bytes,
        ISet<string> existingSourceIds,
        ICollection<ValidationIssue> issues)
    {
        var accepted = 0;
        var evidenceRequired = 0;
        var evidencePresent = 0;
        var sourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var records = new List<VatSubmissionRecord>();

        using var stream = new MemoryStream(bytes);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var row = csv.Parser.Row;
            var contractVersion = csv.GetField("contract_version");
            var sourceRecordId = csv.GetField("source_record_id") ?? string.Empty;
            var registration = csv.GetField("company_registration_number") ?? string.Empty;
            var periodStartText = csv.GetField("tax_period_start");
            var periodEndText = csv.GetField("tax_period_end");
            var outputVatText = csv.GetField("output_vat");
            var inputVatText = csv.GetField("input_vat");
            var vatRefundClaimedText = csv.GetField("vat_refund_claimed");
            var declaredTurnoverText = csv.GetField("declared_turnover");
            var currency = csv.GetField("currency");
            var submissionDateText = csv.GetField("submission_date");
            var vat201Reference = csv.GetField("vat201_reference") ?? string.Empty;
            var paymentDateText = csv.GetField("payment_date");
            var paymentReference = csv.GetField("payment_reference") ?? string.Empty;

            if (!string.Equals(contractVersion, "v1", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ValidationIssue(row, "contract_version", "DC002", "Unsupported contract version."));
            }

            if (string.IsNullOrWhiteSpace(sourceRecordId))
            {
                issues.Add(new ValidationIssue(row, "source_record_id", "DC006", "source_record_id is required."));
            }
            else if (!sourceIds.Add(sourceRecordId))
            {
                issues.Add(new ValidationIssue(row, "source_record_id", "DC008", "Duplicate source_record_id in file."));
            }
            else if (existingSourceIds.Contains(sourceRecordId))
            {
                issues.Add(new ValidationIssue(row, "source_record_id", "DC014", "Duplicate source_record_id already exists for this company."));
            }

            if (!string.Equals(registration, expectedRegistration, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ValidationIssue(row, "company_registration_number", "DC009", "Unknown or mismatched company registration number."));
            }

            DateOnly periodStart = default;
            DateOnly periodEnd = default;
            DateOnly submissionDate = default;
            DateOnly paymentDate = default;
            decimal outputVat = default;
            decimal inputVat = default;
            decimal vatRefundClaimed = default;
            decimal declaredTurnover = default;

            var parseOk =
                DateOnly.TryParseExact(periodStartText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out periodStart) &&
                DateOnly.TryParseExact(periodEndText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out periodEnd) &&
                DateOnly.TryParseExact(submissionDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out submissionDate) &&
                DateOnly.TryParseExact(paymentDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out paymentDate) &&
                decimal.TryParse(outputVatText, NumberStyles.Number, CultureInfo.InvariantCulture, out outputVat) &&
                decimal.TryParse(inputVatText, NumberStyles.Number, CultureInfo.InvariantCulture, out inputVat) &&
                decimal.TryParse(vatRefundClaimedText, NumberStyles.Number, CultureInfo.InvariantCulture, out vatRefundClaimed) &&
                decimal.TryParse(declaredTurnoverText, NumberStyles.Number, CultureInfo.InvariantCulture, out declaredTurnover);

            if (!parseOk)
            {
                issues.Add(new ValidationIssue(row, "row", "DC004", "Invalid VAT submission row format."));
                continue;
            }

            evidenceRequired += 3;
            if (!string.IsNullOrWhiteSpace(vat201Reference)) evidencePresent++;
            if (!string.IsNullOrWhiteSpace(paymentReference)) evidencePresent++;
            if (DateOnly.TryParseExact(paymentDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) evidencePresent++;

            if (string.IsNullOrWhiteSpace(vat201Reference))
            {
                issues.Add(new ValidationIssue(row, "vat201_reference", "DC011", "Missing statutory evidence field."));
            }

            if (string.IsNullOrWhiteSpace(paymentReference))
            {
                issues.Add(new ValidationIssue(row, "payment_reference", "DC011", "Missing statutory evidence field."));
            }

            if (issues.Any(x => x.RowNumber == row))
            {
                continue;
            }

            if (periodEnd < periodStart || paymentDate < periodEnd)
            {
                issues.Add(new ValidationIssue(row, "payment_date", "DC013", "Invalid VAT timeline in submission."));
                continue;
            }

            records.Add(new VatSubmissionRecord
            {
                CompanyId = companyId,
                UploadJobId = uploadJobId,
                SourceRecordId = sourceRecordId,
                TaxPeriodStart = periodStart,
                TaxPeriodEnd = periodEnd,
                OutputVat = outputVat,
                InputVat = inputVat,
                VatRefundClaimed = vatRefundClaimed,
                DeclaredTurnover = declaredTurnover,
                Currency = string.IsNullOrWhiteSpace(currency) ? "ZAR" : currency!,
                SubmissionDate = submissionDate,
                Vat201Reference = vat201Reference,
                PaymentDate = paymentDate,
                PaymentReference = paymentReference
            });
            accepted++;
        }

        var completeness = evidenceRequired == 0 ? 100 : (int)Math.Round((double)evidencePresent * 100 / evidenceRequired);
        return new ParseResult<VatSubmissionRecord>(records, new ParseSummary(accepted, completeness));
    }

    private static string ComputePayloadHash(Guid companyId, DatasetType datasetType, IReadOnlyCollection<byte> bytes)
    {
        var prefix = Encoding.UTF8.GetBytes($"{companyId}|{datasetType}|");
        var combined = new byte[prefix.Length + bytes.Count];
        Buffer.BlockCopy(prefix, 0, combined, 0, prefix.Length);
        bytes.ToArray().CopyTo(combined, prefix.Length);
        return Convert.ToHexString(SHA256.HashData(combined));
    }

    private sealed record ParseSummary(int AcceptedRows, int EvidenceCompleteness);

    private sealed record ParseResult<T>(List<T> Records, ParseSummary Summary);
}
