# TaxTrack Secure Coding Standards v1

Verified against primary sources on 2026-03-05.

## Source Baseline

1. OWASP ASVS 5.0: https://owasp.org/www-project-application-security-verification-standard/
2. OWASP API Security Top 10 (2023): https://owasp.org/API-Security/editions/2023/en/0x11-t10/
3. NIST SP 800-218 (SSDF): https://csrc.nist.gov/pubs/sp/800/218/final
4. RFC 8725 (JWT Best Current Practices): https://www.rfc-editor.org/rfc/rfc8725
5. MITRE CWE Top 25: https://cwe.mitre.org/top25/
6. ASP.NET Core security guidance:
   - https://learn.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-9.0
   - https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0

## Mandatory Engineering Rules

1. Validate all external input with allowlists and deterministic validation errors.
2. Enforce authentication and company-scoped authorization on every protected endpoint.
3. Validate JWT issuer, audience, signature algorithm, expiry, and key strength.
4. Use short-lived access tokens and rotate refresh tokens; store refresh tokens as hashes only.
5. Use HTTPS only, HSTS in production, and no plaintext sensitive data in logs.
6. Never hardcode secrets; load from environment or managed secret stores.
7. Apply rate limiting to authentication and API endpoints.
8. Keep business logic out of controllers; keep contracts and domain logic explicit and typed.
9. Return RFC 7807 problem details for API errors; do not leak stack traces.
10. Produce immutable audit logs for login, upload, analysis, report, and privacy workflows.

## Implementation Guardrails (.NET)

1. Use parameterized ORM/database access only (EF Core LINQ/parameters).
2. Keep nullable reference types enabled and fix nullable warnings.
3. Prefer explicit DTOs over entity binding to avoid over-posting/mass-assignment risk.
4. Enforce idempotency keys on non-idempotent upload/analyze endpoints.
5. Version policy values by effective date (tax rates, due-date parameters, thresholds).

## Required Quality Gates

1. `dotnet build` passes with zero errors.
2. `dotnet test` passes for unit/integration suites.
3. OpenAPI contract validation passes.
4. Security checklist in `docs/7-security-baseline.md` is re-checked on each security-impacting PR.
