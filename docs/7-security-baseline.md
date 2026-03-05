# Security Baseline v1

## Security Objectives

1. Protect financial and identity data confidentiality.
2. Enforce strong authentication and authorization.
3. Ensure traceable, immutable audit events.
4. Reduce common web attack exposure to OWASP ASVS Level 2 baseline.

## Normative References

- OWASP ASVS 5.0
- OWASP API Security Top 10 (2023)
- NIST SP 800-218 (SSDF)
- RFC 8725 (JWT Best Current Practices)
- MITRE CWE Top 25
- ASP.NET Core security guidance (`learn.microsoft.com`)

## Mandatory Controls

### Authentication

- Password hashing with BCrypt.
- Access token: JWT bearer, 15-minute expiry.
- Refresh token: 7-day expiry, rotation on refresh.
- Refresh tokens are stored only as one-way hashes.
- Failed login attempts are rate-limited and logged.

### Authorization

- Role and company-membership checks on all protected endpoints.
- Deny-by-default policy.
- Company-scoped data filters in repository and query logic.

### Transport Security

- HTTPS only.
- HSTS enabled in production.
- TLS 1.2+ only.

### Secrets Management

- Secrets only from encrypted environment variables or managed secret stores.
- No secrets in source control.
- Key rotation policy documented and tested.

### Input And File Safety

- CSV content type and extension validation.
- Max file size and row limits enforced.
- Validation pipeline before persistence.

### Audit Logging

Audit events required for:

- login success/failure
- file upload attempt/result
- risk analysis request/result
- report generation/download
- data export/delete requests

Audit records must be append-only and include actor, timestamp, company scope, and correlation ID.

### POPIA-Specific Security Controls

- POPIA section 21: operator agreements must define security obligations and breach escalation duties.
- POPIA section 22: all security compromises must be assessed and notified as soon as reasonably possible, with regulator/data-subject communication tracking.
- POPIA section 72: cross-border transfers require lawful transfer basis and equivalent safeguards.
- POPIA section 71: substantial-impact automated decisions require explainability and representation path.
- Information Officer registration and delegation records must be maintained and reviewed annually.

## OWASP ASVS L2 Mapping

| Control Area | TaxTrack Baseline | ASVS Reference |
|---|---|---|
| Architecture and threat modeling | documented architecture and security controls | V1 |
| Authentication | BCrypt, token expiry, refresh rotation | V2 |
| Session management | JWT validation, revocation strategy | V3 |
| Access control | membership-based authorization | V4 |
| Validation and encoding | strict CSV and API validation | V5 |
| Stored cryptography | encrypted transport and secret management | V6 |
| Error handling and logging | Problem Details + audit logs | V7, V10 |
| Data protection | scoped data access and retention policy | V8, V9 |
| API security | OpenAPI-defined contracts and auth | V14 |

## Security Test Baseline

1. Invalid JWT rejected with `401`.
2. Expired refresh token rejected with `401`.
3. Non-member access rejected with `403`.
4. Malformed CSV rejected with deterministic validation errors.
5. Idempotency conflicts return `409`.
6. Critical actions produce audit log entries.
7. Section 22 compromise-notification workflow is testable end-to-end.
8. Section 72 transfer controls block unsupported jurisdictions/processors.
9. Section 71 explainability and challenge workflow is available for affected users.

## Operational Requirements

1. Production keys rotated at least every 90 days.
2. Backup snapshots encrypted and tested monthly.
3. Incident response contacts and escalation matrix maintained.
4. Information Officer registration state and details are reviewed at least annually.
