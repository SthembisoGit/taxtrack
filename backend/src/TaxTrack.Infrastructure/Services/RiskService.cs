using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Application.Utils;
using TaxTrack.Domain.Common;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;
using TaxTrack.Infrastructure.Options;

namespace TaxTrack.Infrastructure.Services;

public sealed class RiskService(
    TaxTrackDbContext dbContext,
    ICompanyAccessService companyAccessService,
    IAuditService auditService,
    IOptions<TaxPolicyOptions> taxPolicyOptions,
    ILogger<RiskService> logger) : IRiskService
{
    private readonly TaxPolicyOptions _policy = taxPolicyOptions.Value;

    public async Task<AnalyzeAcceptedResponse> AnalyzeAsync(
        AnalyzeRiskCommand command,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var canAccess = await companyAccessService.CanAccessCompanyAsync(command.UserId, command.CompanyId, cancellationToken);
        if (!canAccess)
        {
            throw new ForbiddenException("User cannot analyze this company.");
        }

        var requestHash = RequestHashing.ComputeHash(new
        {
            command.CompanyId,
            command.PeriodStart,
            command.PeriodEnd
        });

        var existingIdempotency = await dbContext.IdempotencyRecords.FirstOrDefaultAsync(
            x => x.UserId == command.UserId &&
                 x.Endpoint == "POST:/api/risk/analyze" &&
                 x.IdempotencyKey == command.IdempotencyKey,
            cancellationToken);

        if (existingIdempotency is not null)
        {
            if (!string.Equals(existingIdempotency.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new ConflictException("Idempotency key already used with a different payload.");
            }

            var existingJob = await dbContext.RiskAnalysisJobs.FirstAsync(x => x.Id == existingIdempotency.ResourceId, cancellationToken);
            return new AnalyzeAcceptedResponse(existingJob.Id, existingJob.CompanyId, existingJob.Status, existingJob.CreatedAtUtc);
        }

        var job = new RiskAnalysisJob
        {
            CompanyId = command.CompanyId,
            RequestedByUserId = command.UserId,
            PeriodStart = command.PeriodStart,
            PeriodEnd = command.PeriodEnd,
            Status = RiskAnalysisJobStatus.Processing
        };
        dbContext.RiskAnalysisJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.IdempotencyRecords.Add(new IdempotencyRecord
        {
            UserId = command.UserId,
            Endpoint = "POST:/api/risk/analyze",
            IdempotencyKey = command.IdempotencyKey,
            RequestHash = requestHash,
            ResourceId = job.Id
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Risk analysis started {JobId} {CompanyId} {CorrelationId}",
            job.Id,
            command.CompanyId,
            correlationId);

        await auditService.LogAsync(
            command.UserId,
            command.CompanyId,
            AuditEventType.RiskAnalysisRequested,
            correlationId,
            new { jobId = job.Id, command.PeriodStart, command.PeriodEnd },
            ipAddress,
            userAgent,
            cancellationToken);

        var start = command.PeriodStart;
        var end = command.PeriodEnd;

        var txQuery = dbContext.FinancialTransactions.Where(x => x.CompanyId == command.CompanyId);
        var payrollQuery = dbContext.PayrollRecords.Where(x => x.CompanyId == command.CompanyId);
        var vatQuery = dbContext.VatSubmissionRecords.Where(x => x.CompanyId == command.CompanyId);

        if (start.HasValue)
        {
            txQuery = txQuery.Where(x => x.TransactionDate >= start.Value);
            payrollQuery = payrollQuery.Where(x => x.PayPeriod >= start.Value);
            vatQuery = vatQuery.Where(x => x.TaxPeriodStart >= start.Value);
        }

        if (end.HasValue)
        {
            txQuery = txQuery.Where(x => x.TransactionDate <= end.Value);
            payrollQuery = payrollQuery.Where(x => x.PayPeriod <= end.Value);
            vatQuery = vatQuery.Where(x => x.TaxPeriodEnd <= end.Value);
        }

        var transactions = await txQuery.ToListAsync(cancellationToken);
        var payroll = await payrollQuery.ToListAsync(cancellationToken);
        var vatSubmissions = await vatQuery.ToListAsync(cancellationToken);
        var priorResults = await dbContext.TaxRiskResults
            .Where(x => x.CompanyId == command.CompanyId)
            .OrderByDescending(x => x.GeneratedAtUtc)
            .Take(3)
            .ToListAsync(cancellationToken);

        var outcomes = EvaluateRules(transactions, payroll, vatSubmissions, priorResults);

        var rawRegulatory = outcomes.Where(x => x.RuleClass == RuleClass.Regulatory && x.Triggered).Sum(x => x.Weight);
        var rawHeuristic = outcomes.Where(x => x.RuleClass == RuleClass.Heuristic && x.Triggered).Sum(x => x.Weight);
        var regulatoryScore = Math.Min(70, (int)Math.Round(rawRegulatory * 0.7m));
        var heuristicScore = Math.Min(30, rawHeuristic);
        var riskScore = Math.Min(100, regulatoryScore + heuristicScore);
        var riskLevel = DetermineRiskLevel(riskScore);

        var evidenceCompleteness = await ComputeEvidenceCompletenessAsync(command.CompanyId, cancellationToken);
        var insufficientEvidence = evidenceCompleteness < 80;

        if (insufficientEvidence)
        {
            logger.LogWarning(
                "Risk analysis evidence incomplete {JobId} {CompanyId} {EvidenceCompleteness} {CorrelationId}",
                job.Id,
                command.CompanyId,
                evidenceCompleteness,
                correlationId);
        }

        var result = new TaxRiskResult
        {
            CompanyId = command.CompanyId,
            RiskScore = riskScore,
            RegulatoryScore = regulatoryScore,
            HeuristicScore = heuristicScore,
            RiskLevel = riskLevel,
            TaxPolicyVersion = _policy.Version,
            PolicyEffectiveDate = _policy.EffectiveDate,
            EvidenceCompleteness = evidenceCompleteness,
            InsufficientEvidence = insufficientEvidence
        };

        dbContext.TaxRiskResults.Add(result);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var outcome in outcomes.Where(x => x.Triggered))
        {
            dbContext.RiskAlerts.Add(new RiskAlert
            {
                RiskResultId = result.Id,
                RuleCode = outcome.RuleCode,
                RuleClass = outcome.RuleClass,
                Type = outcome.RuleClass.ToString(),
                Description = outcome.Description,
                Severity = outcome.Severity,
                Recommendation = outcome.Recommendation,
                EvidenceJson = JsonSerializer.Serialize(outcome.Evidence)
            });
        }

        job.Status = RiskAnalysisJobStatus.Completed;
        job.ResultId = result.Id;
        job.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Risk analysis completed {JobId} {CompanyId} {RiskScore} {CorrelationId}",
            job.Id,
            command.CompanyId,
            riskScore,
            correlationId);

        await auditService.LogAsync(
            command.UserId,
            command.CompanyId,
            AuditEventType.RiskAnalysisCompleted,
            correlationId,
            new { jobId = job.Id, resultId = result.Id, riskScore, regulatoryScore, heuristicScore },
            ipAddress,
            userAgent,
            cancellationToken);

        return new AnalyzeAcceptedResponse(job.Id, job.CompanyId, job.Status, job.CreatedAtUtc);
    }

    public async Task<RiskAnalysisJobStatusResponse?> GetStatusAsync(Guid userId, Guid analysisId, CancellationToken cancellationToken)
    {
        var job = await dbContext.RiskAnalysisJobs.FirstOrDefaultAsync(x => x.Id == analysisId, cancellationToken);
        if (job is null)
        {
            return null;
        }

        var canAccess = await companyAccessService.CanAccessCompanyAsync(userId, job.CompanyId, cancellationToken);
        if (!canAccess)
        {
            throw new ForbiddenException("User cannot access this analysis job.");
        }

        return new RiskAnalysisJobStatusResponse(job.Id, job.CompanyId, job.Status, job.ResultId, job.UpdatedAtUtc);
    }

    public async Task<RiskResultResponse?> GetLatestResultAsync(Guid userId, Guid companyId, CancellationToken cancellationToken)
    {
        var canAccess = await companyAccessService.CanAccessCompanyAsync(userId, companyId, cancellationToken);
        if (!canAccess)
        {
            throw new ForbiddenException("User cannot access this company risk.");
        }

        var result = await dbContext.TaxRiskResults
            .Include(x => x.Alerts)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.GeneratedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

        return MapResult(result);
    }

    private List<RuleOutcome> EvaluateRules(
        IReadOnlyCollection<FinancialTransaction> transactions,
        IReadOnlyCollection<PayrollRecord> payroll,
        IReadOnlyCollection<VatSubmissionRecord> vatSubmissions,
        IReadOnlyCollection<TaxRiskResult> priorResults)
    {
        var outcomes = new List<RuleOutcome>();

        var totalPaye = payroll.Sum(x => x.TotalPaye);
        var declaredPaye = payroll.Sum(x => x.Emp201DeclaredPaye);
        var payeMismatch = totalPaye > 0 && Math.Abs(totalPaye - declaredPaye) / totalPaye > 0.05m;
        outcomes.Add(new RuleOutcome(
            RuleCodes.PayeReg001,
            RuleClass.Regulatory,
            30,
            payeMismatch,
            AlertSeverity.Critical,
            "Declared EMP201 PAYE does not reconcile with payroll-calculated PAYE.",
            "Reconcile EMP201 PAYE totals against payroll source records before submission.",
            new { totalPaye, declaredPaye }));

        var cycleDeclaredPaye = payroll.Sum(x => x.Emp201DeclaredPaye);
        var cycleIrp5 = payroll.Select(x => x.Irp5TotalPaye).DefaultIfEmpty(0).Max();
        var cycleMismatch = cycleIrp5 > 0 && Math.Abs(cycleDeclaredPaye - cycleIrp5) / cycleIrp5 > 0.01m;
        outcomes.Add(new RuleOutcome(
            RuleCodes.PayeReg002,
            RuleClass.Regulatory,
            25,
            cycleMismatch,
            AlertSeverity.Critical,
            "EMP201, EMP501, and IRP5 totals do not reconcile.",
            "Resolve EMP501 and IRP5 reconciliation differences before filing cycle close.",
            new { cycleDeclaredPaye, cycleIrp5 }));

        var hasLatePaye = payroll.Any(x => x.SubmissionDate > GetEmp201DueDate(x.Emp201Period) || x.PaymentDate > GetEmp201DueDate(x.Emp201Period));
        outcomes.Add(new RuleOutcome(
            RuleCodes.PayeReg003,
            RuleClass.Regulatory,
            15,
            hasLatePaye,
            AlertSeverity.Warning,
            "PAYE filing or payment appears late.",
            "Review filing/payment timetable and settle overdue items.",
            new { lateRows = payroll.Count(x => x.SubmissionDate > GetEmp201DueDate(x.Emp201Period) || x.PaymentDate > GetEmp201DueDate(x.Emp201Period)) }));

        var txInput = transactions.Where(x => x.LedgerCategory.Equals("VATInput", StringComparison.OrdinalIgnoreCase)).Sum(x => x.VatAmount);
        var txOutput = transactions.Where(x => x.LedgerCategory.Equals("VATOutput", StringComparison.OrdinalIgnoreCase)).Sum(x => x.VatAmount);
        var vatInput = vatSubmissions.Sum(x => x.InputVat);
        var vatOutput = vatSubmissions.Sum(x => x.OutputVat);
        var vatMismatch = (vatInput > 0 && Math.Abs(vatInput - txInput) / vatInput > 0.05m) ||
                          (vatOutput > 0 && Math.Abs(vatOutput - txOutput) / vatOutput > 0.05m);
        outcomes.Add(new RuleOutcome(
            RuleCodes.VatReg001,
            RuleClass.Regulatory,
            20,
            vatMismatch,
            AlertSeverity.Warning,
            "VAT201 values do not reconcile with VAT-tagged ledger totals.",
            "Reconcile VATInput and VATOutput ledger totals with VAT201 before submission.",
            new { vatInput, txInput, vatOutput, txOutput }));

        var missingEvidenceCount = transactions.Count(x =>
            x.LedgerCategory.Equals("VATInput", StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrWhiteSpace(x.TaxInvoiceNumber) ||
             string.IsNullOrWhiteSpace(x.SupplierVatNumber) ||
             x.TaxInvoiceDate is null));
        outcomes.Add(new RuleOutcome(
            RuleCodes.VatReg002,
            RuleClass.Regulatory,
            10,
            missingEvidenceCount > 0,
            AlertSeverity.Warning,
            "VAT input claims contain missing documentary evidence.",
            "Attach missing tax invoice fields before next VAT cycle.",
            new { missingEvidenceCount }));

        var refundRatio = vatSubmissions.Sum(x => x.DeclaredTurnover) > 0
            ? vatSubmissions.Sum(x => x.VatRefundClaimed) / vatSubmissions.Sum(x => x.DeclaredTurnover)
            : 0m;
        outcomes.Add(new RuleOutcome(
            RuleCodes.VatHeu001,
            RuleClass.Heuristic,
            12,
            refundRatio > 0.30m,
            AlertSeverity.Warning,
            "VAT refund ratio is unusually high relative to turnover.",
            "Review high VAT refund contributors and classify supporting evidence.",
            new { refundRatio }));

        var expenses = transactions.Where(x => x.Direction.Equals("debit", StringComparison.OrdinalIgnoreCase)).ToList();
        var entertainment = expenses.Where(x => x.LedgerCategory.Equals("Entertainment", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);
        var totalExpense = expenses.Sum(x => x.Amount);
        var entertainmentRatio = totalExpense > 0 ? entertainment / totalExpense : 0m;
        outcomes.Add(new RuleOutcome(
            RuleCodes.ExpHeu001,
            RuleClass.Heuristic,
            8,
            entertainmentRatio > 0.08m,
            AlertSeverity.Warning,
            "Entertainment expense ratio is above expected threshold.",
            "Review entertainment classification and business purpose documentation.",
            new { entertainmentRatio }));

        var monthlyRevenue = transactions
            .Where(x => x.LedgerCategory.Equals("Revenue", StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(x => x.Sum(y => y.Amount))
            .ToList();
        var volatility = 0m;
        for (var i = 1; i < monthlyRevenue.Count; i++)
        {
            if (monthlyRevenue[i - 1] == 0) continue;
            var ratio = Math.Abs(monthlyRevenue[i] - monthlyRevenue[i - 1]) / monthlyRevenue[i - 1];
            volatility = Math.Max(volatility, ratio);
        }

        outcomes.Add(new RuleOutcome(
            RuleCodes.RevHeu001,
            RuleClass.Heuristic,
            5,
            monthlyRevenue.Count >= 2 && volatility > 0.40m,
            AlertSeverity.Warning,
            "Revenue declaration volatility is unusually high month-over-month.",
            "Validate revenue recognition and declaration consistency.",
            new { volatility }));

        var roundNumberCount = expenses.Count(x => x.Amount % 100 == 0);
        var roundRatio = expenses.Count == 0 ? 0m : (decimal)roundNumberCount / expenses.Count;
        outcomes.Add(new RuleOutcome(
            RuleCodes.ExpHeu002,
            RuleClass.Heuristic,
            3,
            roundRatio > 0.35m,
            AlertSeverity.Info,
            "Expense values show unusual round-number clustering.",
            "Investigate repeated rounded expense patterns and controls.",
            new { roundRatio }));

        var repeatedAnomalies = priorResults.Count >= 3 && priorResults.All(x => x.RiskScore > 40);
        outcomes.Add(new RuleOutcome(
            RuleCodes.RepHeu001,
            RuleClass.Heuristic,
            2,
            repeatedAnomalies,
            AlertSeverity.Warning,
            "Repeated anomalies detected across recent periods.",
            "Prioritize corrective actions and track closure over multiple periods.",
            new { priorCount = priorResults.Count }));

        return outcomes;
    }

    private async Task<int> ComputeEvidenceCompletenessAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var latestJobs = await dbContext.UploadJobs
            .Where(x => x.CompanyId == companyId && x.Status == UploadJobStatus.Completed)
            .GroupBy(x => x.DatasetType)
            .Select(g => g.OrderByDescending(x => x.UpdatedAtUtc).First())
            .ToListAsync(cancellationToken);

        if (latestJobs.Count == 0)
        {
            return 0;
        }

        return (int)Math.Round(latestJobs.Average(x => x.EvidenceCompleteness));
    }

    internal static RiskLevel DetermineRiskLevel(int riskScore)
    {
        return riskScore switch
        {
            <= 40 => RiskLevel.Low,
            <= 70 => RiskLevel.Medium,
            _ => RiskLevel.High
        };
    }

    private DateOnly GetEmp201DueDate(DateOnly period)
    {
        var nextMonth = period.AddMonths(1);
        var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        var dueDay = Math.Clamp(_policy.Emp201DueDayOfMonth, 1, daysInMonth);
        var dueDate = new DateOnly(nextMonth.Year, nextMonth.Month, dueDay);

        // Align due date to the previous business day when configured due date is non-business.
        while (IsNonBusinessDay(dueDate))
        {
            dueDate = dueDate.AddDays(-1);
        }

        return dueDate;
    }

    private bool IsNonBusinessDay(DateOnly value)
    {
        if (value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return true;
        }

        return _policy.PublicHolidayDates?.Contains(value) == true;
    }

    private static RiskResultResponse MapResult(TaxRiskResult result)
    {
        var alerts = result.Alerts
            .Select(x => new RiskAlertResponse(
                x.RuleCode,
                x.RuleClass,
                x.Type,
                x.Description,
                x.Severity,
                x.Recommendation,
                x.EvidenceJson))
            .ToArray();

        return new RiskResultResponse(
            result.Id,
            result.CompanyId,
            result.RiskScore,
            result.RegulatoryScore,
            result.HeuristicScore,
            result.RiskLevel,
            result.TaxPolicyVersion,
            result.PolicyEffectiveDate,
            result.EvidenceCompleteness,
            result.InsufficientEvidence,
            alerts,
            result.GeneratedAtUtc);
    }

    private sealed record RuleOutcome(
        string RuleCode,
        RuleClass RuleClass,
        int Weight,
        bool Triggered,
        AlertSeverity Severity,
        string Description,
        string Recommendation,
        object Evidence);
}
