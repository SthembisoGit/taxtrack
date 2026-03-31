# TaxTrack MVP Test Plan

## Test Strategy

Testing is aligned to contracts, deterministic rule logic, and security baseline.

## Test Levels

### Unit Tests

- Rule trigger logic per rule code.
- Score calculation and risk-level boundaries.
- Validation rule functions and error code mapping.

### Integration Tests

- Auth flow (`register -> login -> authorized request`).
- Upload flow (`upload -> validate -> persist`).
- Analysis flow (`analyze -> score -> alerts`).
- Report flow (`generate -> response consistency`).

### Contract Tests

- OpenAPI response shape conformance.
- CSV contract conformance for all sample files.
- Enum contract stability (`RiskLevel`, `AlertSeverity`, `RuleClass`).
- Policy metadata contract stability (`taxPolicyVersion`, `policyEffectiveDate`).
- Privacy request contract tests for export/deletion workflows.

### Security Tests

- Invalid and expired token rejection.
- Cross-company access denial.
- Idempotency conflict handling.
- Audit event generation for sensitive actions.
- POPIA section 22 breach-notification SLA/channel flow.
- POPIA section 72 cross-border transfer control enforcement.
- POPIA section 71 automated decision representation/human-review workflow.

### UI Tests

- Risk badge semantic color mapping.
- Empty/loading/error/success state coverage.
- Dashboard/report score consistency.
- WCAG AA contrast checks for text and alert badges.

## Required MVP Scenarios

1. CSV contract valid file accepted (`202`).
2. CSV contract invalid file rejected (`422`) with row-level errors.
3. Rule weight aggregation and score cap at 100.
4. Boundary mapping tests at 40/41 and 70/71.
5. Duplicate idempotency key does not duplicate processing.
6. Membership-based access control for accountant multi-company model.
7. Audit logs generated for upload, analysis requested/completed, and successful report download.
8. Refresh token misuse and expiry behavior verified.
9. Dashboard and report return identical score and risk level.
10. VAT policy effective-date regression test handles rate-policy changes and reversals.
11. PAYE reconciliation mismatch test across payroll, EMP201, EMP501, and IRP5 totals.
12. VAT refund evidence completeness test sets `insufficientEvidence` when required evidence is missing.
13. Scoring stability test confirms deterministic fallback behavior with partial evidence.
14. Privacy request lifecycle test validates `Received -> InProgress -> Completed|Rejected`.
15. EMP201 due-date calculation test covers weekend/public-holiday fallback behavior.
16. Refresh-token rotation test rejects replay of revoked refresh tokens.

## Test Data

- Use files under `/samples` as base fixtures.
- Extend with synthetic datasets for threshold boundaries.
- Tag fixtures by rule coverage to ensure deterministic behavior.

## Entry And Exit Criteria

### Entry

- Contracts frozen in docs and OpenAPI.
- Required fixtures available.
- Environment secrets configured.

### Exit

- No critical or high severity unresolved defects.
- All required scenarios pass.
- Security baseline checks pass.
- Regression report attached to release candidate.
