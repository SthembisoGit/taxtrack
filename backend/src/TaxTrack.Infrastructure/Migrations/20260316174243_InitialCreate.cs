using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    EventTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "text", nullable: false),
                    Industry = table.Column<string>(type: "text", nullable: false),
                    TaxReference = table.Column<string>(type: "text", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataSubjectRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ResolutionNote = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSubjectRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceRecordId = table.Column<string>(type: "text", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LedgerCategory = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    VatAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    SourceSystem = table.Column<string>(type: "text", nullable: false),
                    TaxInvoiceNumber = table.Column<string>(type: "text", nullable: true),
                    SupplierVatNumber = table.Column<string>(type: "text", nullable: true),
                    TaxInvoiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Vat201Reference = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdempotencyRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Endpoint = table.Column<string>(type: "text", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "text", nullable: false),
                    RequestHash = table.Column<string>(type: "text", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceRecordId = table.Column<string>(type: "text", nullable: false),
                    PayPeriod = table.Column<DateOnly>(type: "date", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPaye = table.Column<decimal>(type: "numeric", nullable: false),
                    EmployeeCount = table.Column<int>(type: "integer", nullable: false),
                    TotalUif = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSdl = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Emp201Period = table.Column<DateOnly>(type: "date", nullable: false),
                    Emp201DeclaredPaye = table.Column<decimal>(type: "numeric", nullable: false),
                    Emp201DeclaredUif = table.Column<decimal>(type: "numeric", nullable: false),
                    Emp201DeclaredSdl = table.Column<decimal>(type: "numeric", nullable: false),
                    Emp501Reference = table.Column<string>(type: "text", nullable: false),
                    Irp5TotalPaye = table.Column<decimal>(type: "numeric", nullable: false),
                    SubmissionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentReference = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskAnalysisJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResultId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: true),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAnalysisJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRiskResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    RegulatoryScore = table.Column<int>(type: "integer", nullable: false),
                    HeuristicScore = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    TaxPolicyVersion = table.Column<string>(type: "text", nullable: false),
                    PolicyEffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EvidenceCompleteness = table.Column<int>(type: "integer", nullable: false),
                    InsufficientEvidence = table.Column<bool>(type: "boolean", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRiskResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UploadJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DatasetType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AcceptedRows = table.Column<int>(type: "integer", nullable: false),
                    RejectedRows = table.Column<int>(type: "integer", nullable: false),
                    EvidenceCompleteness = table.Column<int>(type: "integer", nullable: false),
                    InsufficientEvidence = table.Column<bool>(type: "boolean", nullable: false),
                    ValidationErrorsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VatSubmissionRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceRecordId = table.Column<string>(type: "text", nullable: false),
                    TaxPeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    TaxPeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    OutputVat = table.Column<decimal>(type: "numeric", nullable: false),
                    InputVat = table.Column<decimal>(type: "numeric", nullable: false),
                    VatRefundClaimed = table.Column<decimal>(type: "numeric", nullable: false),
                    DeclaredTurnover = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    SubmissionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Vat201Reference = table.Column<string>(type: "text", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentReference = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatSubmissionRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "text", nullable: false),
                    RuleClass = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Recommendation = table.Column<string>(type: "text", nullable: false),
                    EvidenceJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAlerts_TaxRiskResults_RiskResultId",
                        column: x => x.RiskResultId,
                        principalTable: "TaxRiskResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyMemberships_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_RegistrationNumber",
                table: "Companies",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMemberships_CompanyId_UserId",
                table: "CompanyMemberships",
                columns: new[] { "CompanyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMemberships_UserId",
                table: "CompanyMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CompanyId_SourceRecordId",
                table: "FinancialTransactions",
                columns: new[] { "CompanyId", "SourceRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_UserId_Endpoint_IdempotencyKey",
                table: "IdempotencyRecords",
                columns: new[] { "UserId", "Endpoint", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_CompanyId_SourceRecordId",
                table: "PayrollRecords",
                columns: new[] { "CompanyId", "SourceRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAlerts_RiskResultId",
                table: "RiskAlerts",
                column: "RiskResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VatSubmissionRecords_CompanyId_SourceRecordId",
                table: "VatSubmissionRecords",
                columns: new[] { "CompanyId", "SourceRecordId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogEvents");

            migrationBuilder.DropTable(
                name: "CompanyMemberships");

            migrationBuilder.DropTable(
                name: "DataSubjectRequests");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "IdempotencyRecords");

            migrationBuilder.DropTable(
                name: "PayrollRecords");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RiskAlerts");

            migrationBuilder.DropTable(
                name: "RiskAnalysisJobs");

            migrationBuilder.DropTable(
                name: "UploadJobs");

            migrationBuilder.DropTable(
                name: "VatSubmissionRecords");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "TaxRiskResults");
        }
    }
}
