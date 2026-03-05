namespace TaxTrack.Domain.Common;

public enum UserRole
{
    Owner = 1,
    Accountant = 2,
    FinanceManager = 3,
    Viewer = 4
}

public enum MembershipRole
{
    Owner = 1,
    Manager = 2,
    Accountant = 3,
    Viewer = 4
}

public enum DatasetType
{
    Company = 1,
    Transactions = 2,
    Payroll = 3,
    VatSubmissions = 4
}

public enum UploadJobStatus
{
    Queued = 1,
    Validating = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5
}

public enum RiskAnalysisJobStatus
{
    Queued = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3
}

public enum AlertSeverity
{
    Info = 1,
    Warning = 2,
    Critical = 3
}

public enum RuleClass
{
    Regulatory = 1,
    Heuristic = 2
}

public enum DataSubjectRequestType
{
    Export = 1,
    Deletion = 2
}

public enum DataSubjectRequestStatus
{
    Received = 1,
    InProgress = 2,
    Completed = 3,
    Rejected = 4
}

public enum AuditEventType
{
    LoginSucceeded = 1,
    LoginFailed = 2,
    UploadCreated = 3,
    UploadValidationFailed = 4,
    RiskAnalysisRequested = 5,
    RiskAnalysisCompleted = 6,
    ReportDownloaded = 7,
    DataExportRequested = 8,
    DataDeletionRequested = 9,
    DataBreachNotified = 10,
    TokenRefreshed = 11
}
