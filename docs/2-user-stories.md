# TaxTrack MVP User Stories

All stories include measurable acceptance criteria and are considered "ready" only when criteria are testable.

## Story 1: User Registration

As a new user, I want to register with email and password so I can access TaxTrack.

Acceptance criteria:

- `POST /api/auth/register` returns `201` for valid input.
- Email uniqueness is enforced.
- Password is stored only as a BCrypt hash.

## Story 2: User Login

As a registered user, I want to log in and receive tokens so I can use protected endpoints.

Acceptance criteria:

- `POST /api/auth/login` returns access and refresh tokens for valid credentials.
- Invalid credentials return `401` with Problem Details.
- Access token expiry is 15 minutes and refresh token expiry is 7 days.

## Story 3: Create Company Profile

As an authenticated user, I want to create a company profile so I can upload data against that company.

Acceptance criteria:

- `POST /api/company` returns `201` with company ID.
- Duplicate registration number returns `409`.
- Creator is assigned `Owner` membership for that company.

## Story 4: View Company Profile

As a company member, I want to fetch company details so I can confirm profile setup.

Acceptance criteria:

- `GET /api/company/{id}` returns company data for authorized members.
- Non-members receive `403`.
- Unknown company IDs return `404`.

## Story 5: Upload Transactions CSV

As a company user, I want to upload transactions CSV data so risk analysis can run.

Acceptance criteria:

- `POST /api/financial/upload` accepts multipart CSV with `datasetType=transactions`.
- Valid file returns `202` with upload ID and queued status.
- Validation failures return `422` with row-level errors.

## Story 6: Upload Payroll CSV

As a company user, I want to upload payroll CSV data so payroll risk rules can evaluate.

Acceptance criteria:

- Upload with `datasetType=payroll` is accepted for valid files.
- Missing required columns produce deterministic error codes.
- Upload action is audit logged.

## Story 7: Upload VAT Submission CSV

As a company user, I want to upload VAT submissions so VAT rules can evaluate declared values.

Acceptance criteria:

- Upload with `datasetType=vat_submissions` is accepted for valid files.
- Invalid formats return structured `422` errors.
- Upload metadata includes idempotency key linkage.

## Story 8: Trigger Risk Analysis

As a company user, I want to start risk analysis so I can get an updated risk score.

Acceptance criteria:

- `POST /api/risk/analyze` requires idempotency key and company ID.
- Valid request returns `202` with analysis job ID.
- Duplicate idempotency key for same payload does not create a new job.

## Story 9: View Latest Risk Result

As a company user, I want to retrieve the latest score so I can understand current risk level.

Acceptance criteria:

- `GET /api/risk/{companyId}` returns latest `riskScore`, `riskLevel`, and alerts.
- Risk levels strictly map to Low (0-40), Medium (41-70), High (71-100).
- Non-members receive `403`.

## Story 10: VAT Risk Alert Visibility

As a finance manager, I want VAT-related risk alerts to explain why they triggered.

Acceptance criteria:

- Returned alerts include `ruleCode`, `description`, `severity`, and remediation note.
- VAT refund ratio rule triggers when refund exceeds 30 percent of turnover.
- Alert severity maps to `Warning` or `Critical` based on configured threshold.

## Story 11: Payroll Mismatch Alert Visibility

As a finance manager, I want payroll anomalies flagged so I can correct filings early.

Acceptance criteria:

- Payroll-calculated PAYE vs EMP201 declared PAYE mismatch beyond tolerance triggers alert.
- EMP201, EMP501, and IRP5 reconciliation mismatch triggers alert.
- Alert includes declared values, calculated values, and mismatch percentage.
- Alert is included in both dashboard payload and report payload.

## Story 12: Expense Pattern Alert Visibility

As an owner, I want suspicious expense pattern alerts so I can review high-risk categories.

Acceptance criteria:

- Entertainment ratio above threshold triggers expense alert.
- Alert includes category ratio and threshold.
- Rule code is stable and documented.

## Story 13: Download Compliance Report

As a company user, I want to download a compliance report so I can share with leadership or advisors.

Acceptance criteria:

- `GET /api/report/{companyId}` returns report metadata and data payload.
- Report includes score, level, triggered rules, and generated timestamp.
- Report request is audit logged.

## Story 14: Accountant Multi-Company Access

As an accountant, I want access to assigned client companies only so I can manage multiple entities securely.

Acceptance criteria:

- Membership records determine company access.
- Accountant role can access assigned companies only.
- Access outside membership returns `403`.

## Story 15: Audit Trail Visibility

As an admin user, I want audit events recorded for sensitive actions so compliance reviews are possible.

Acceptance criteria:

- Login, upload, analyze, and report download events are persisted.
- Each event stores actor, company, action, timestamp, and correlation ID.
- Events are append-only.

## Story 16: Validation Error Clarity

As an uploader, I want row-level validation errors so I can fix files quickly.

Acceptance criteria:

- Validation responses include row number, column name, error code, and message.
- Error codes map to `docs/3-data-contract.md`.
- At least 100 errors can be returned deterministically for large invalid files.

## Story 17: Branded Risk Dashboard Semantics

As a user, I want risk states color-coded consistently so I can interpret status quickly.

Acceptance criteria:

- Low risk uses success token, medium uses warning token, high uses danger token.
- API returns semantic levels, not color values.
- UI colors come only from `docs/design/tokens-v1.json`.

## Story 18: Data Export/Delete Request Recording

As a data subject user, I want export/delete requests recorded so POPIA obligations are trackable.

Acceptance criteria:

- Requests are captured with requester identity and timestamp.
- Request status lifecycle is tracked (`Received`, `InProgress`, `Completed`, `Rejected`).
- Processing events are audit logged.
