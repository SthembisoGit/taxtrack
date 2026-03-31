# TaxTrack MVP Scope

## In Scope (MVP)

1. User registration and login
2. Company profile creation and retrieval
3. CSV upload for:
   - company profile reference data
   - transactions
   - payroll
   - VAT submissions
4. Data validation and structured error reporting
5. Rule-based risk engine and score calculation (0-100)
6. Dashboard risk summary and alerts list
7. Compliance report generation (JSON response + authenticated downloadable JSON artifact metadata)
8. Audit trail logging for security and traceability

## Out Of Scope (MVP)

1. Direct SARS integration or submission
2. AI/ML risk models
3. Live bank feed integrations
4. Xero, Sage, QuickBooks connectors
5. Fraud analytics beyond documented rule set
6. Native mobile applications
7. Full BI and benchmarking market analytics

## Non-Goals

1. Tax filing automation
2. Tax or legal advisory conclusions
3. Replacing accountants
4. Producing an official SARS audit score or SARS decision output

## Audience And Access Model

- Single-company operational users (owner, finance manager)
- Accountant users who can manage multiple assigned companies

## Constraints

- Implementation window: 8 to 10 weeks
- Stack: .NET 8, React + Vite, PostgreSQL, EF Core
- Security minimum: OWASP ASVS L2 baseline controls
- Privacy baseline: POPIA-aligned data handling
- Tax policy handling: policy values are versioned by effective date (for example VAT rate, VAT registration thresholds, EMP201 due-date policy).

## Regulatory Positioning

1. TaxTrack score is an internal risk heuristic for early warning.
2. TaxTrack risk output is not an official SARS score and does not represent a SARS determination.
3. Heuristic thresholds are internal configuration and may be adjusted as policies and compliance patterns change.

## Scope Control Rules

1. Any feature not mapped to an approved user story is deferred.
2. Any contract change requires updates in `docs/3-data-contract.md` and `docs/api/openapi-v1.yaml`.
3. Any security-impacting change requires update in `docs/7-security-baseline.md`.
4. Any tax policy change must update policy-effective-date configuration, regression tests, and release notes.
