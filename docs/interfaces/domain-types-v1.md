# Domain Types v1

This document defines canonical domain and API-aligned types for MVP.

## Enums

### UserRole

- `Owner`
- `Accountant`
- `FinanceManager`
- `Viewer`

### MembershipRole

- `Owner`
- `Manager`
- `Accountant`
- `Viewer`

### RiskLevel

- `Low`
- `Medium`
- `High`

### AlertSeverity

- `Info`
- `Warning`
- `Critical`

### RuleClass

- `Regulatory`
- `Heuristic`

### UploadJobStatus

- `Queued`
- `Validating`
- `Processing`
- `Completed`
- `Failed`

### RiskAnalysisJobStatus

- `Queued`
- `Processing`
- `Completed`
- `Failed`

### DataSubjectRequestType

- `Export`
- `Deletion`

### DataSubjectRequestStatus

- `Received`
- `InProgress`
- `Completed`
- `Rejected`

### AuditEventType

- `LoginSucceeded`
- `LoginFailed`
- `UploadCreated`
- `UploadValidationFailed`
- `RiskAnalysisRequested`
- `RiskAnalysisCompleted`
- `ReportDownloaded`
- `DataExportRequested`
- `DataDeletionRequested`
- `DataBreachNotified`
- `TokenRefreshed`

## Entities

### User

| Field | Type |
|---|---|
| id | UUID |
| email | string |
| passwordHash | string |
| role | UserRole |
| createdAt | datetime (UTC) |
| lastLoginAt | datetime (UTC, nullable) |

### Company

| Field | Type |
|---|---|
| id | UUID |
| name | string |
| registrationNumber | string |
| industry | string |
| taxReference | string |
| ownerUserId | UUID |
| createdAt | datetime (UTC) |

### CompanyMembership

| Field | Type |
|---|---|
| id | UUID |
| companyId | UUID |
| userId | UUID |
| role | MembershipRole |
| createdAt | datetime (UTC) |
| createdByUserId | UUID |
| isActive | boolean |

### FinancialRecord

| Field | Type |
|---|---|
| id | UUID |
| companyId | UUID |
| sourceRecordId | string |
| datasetType | string (`transactions` \| `payroll` \| `vat_submissions`) |
| recordDate | date |
| category | string |
| amount | decimal(18,2) |
| currency | string |
| source | string |
| uploadedAt | datetime (UTC) |

### TaxRiskResult

| Field | Type |
|---|---|
| id | UUID |
| companyId | UUID |
| riskScore | integer (0-100) |
| regulatoryScore | integer (0-70) |
| heuristicScore | integer (0-30) |
| riskLevel | RiskLevel |
| taxPolicyVersion | string |
| policyEffectiveDate | date |
| evidenceCompleteness | integer (0-100) |
| insufficientEvidence | boolean |
| generatedAt | datetime (UTC) |
| contractVersion | string |

### RiskAlert

| Field | Type |
|---|---|
| id | UUID |
| riskResultId | UUID |
| ruleCode | string |
| ruleClass | RuleClass |
| type | string |
| description | string |
| severity | AlertSeverity |
| recommendation | string |

### DataSubjectRequest

| Field | Type |
|---|---|
| id | UUID |
| requesterUserId | UUID |
| companyId | UUID (nullable) |
| requestType | DataSubjectRequestType |
| status | DataSubjectRequestStatus |
| reason | string |
| createdAt | datetime (UTC) |
| updatedAt | datetime (UTC) |
| resolutionNote | string (nullable) |

### UploadJob

| Field | Type |
|---|---|
| id | UUID |
| companyId | UUID |
| datasetType | string |
| status | UploadJobStatus |
| evidenceCompleteness | integer (0-100) |
| insufficientEvidence | boolean |
| createdAt | datetime (UTC) |
| updatedAt | datetime (UTC) |

### RiskAnalysisJob

| Field | Type |
|---|---|
| id | UUID |
| companyId | UUID |
| status | RiskAnalysisJobStatus |
| resultId | UUID (nullable) |
| createdAt | datetime (UTC) |
| updatedAt | datetime (UTC) |

### AuditLogEvent

| Field | Type |
|---|---|
| id | UUID |
| actorUserId | UUID |
| companyId | UUID (nullable for global events) |
| eventType | AuditEventType |
| eventTime | datetime (UTC) |
| ipAddress | string |
| userAgent | string |
| correlationId | string |
| metadataJson | jsonb |

## Risk Engine Interfaces

### IRiskRule

- `RuleCode: string`
- `RuleClass: RuleClass`
- `Weight: int`
- `Severity: AlertSeverity`
- `Evaluate(context: RiskEvaluationContext): RuleEvaluationOutcome`

### IRiskRuleEvaluator

- `EvaluateAll(context: RiskEvaluationContext): IReadOnlyCollection<RuleEvaluationOutcome>`

### IRiskScorer

- `Score(outcomes: IReadOnlyCollection<RuleEvaluationOutcome>): TaxRiskResult`

### RiskEvaluationContext

| Field | Type |
|---|---|
| companyId | UUID |
| periodStart | date |
| periodEnd | date |
| taxPolicyVersion | string |
| policyEffectiveDate | date |
| transactions | collection |
| payrollRecords | collection |
| vatSubmissions | collection |
| priorResults | collection |

### RuleEvaluationOutcome

| Field | Type |
|---|---|
| ruleCode | string |
| ruleClass | RuleClass |
| triggered | boolean |
| weight | int |
| severity | AlertSeverity |
| description | string |
| recommendation | string |
| evidence | json object |

## API Representation Rules

1. API responses expose semantic enums (`RiskLevel`, `AlertSeverity`, `RuleClass`) only.
2. API responses do not expose UI colors.
3. API and UI must use identical enum value casing and spelling.
4. Risk and report responses must include policy metadata and evidence-quality metadata.
