# Pre-Coding Readiness Checklist

Feature coding is blocked until all gates are approved.

## Approval Gates

| Gate | Required Artifact(s) | Owner | Status | Approved On |
|---|---|---|---|---|
| 1. Scope | `docs/1-scope.md`, `docs/2-user-stories.md` | Product Lead | Accepted | 2026-03-05 |
| 2. Data Contract | `docs/3-data-contract.md`, `/samples/*` | Data Lead | Accepted | 2026-03-05 |
| 3. Risk Rulebook | `docs/4-risk-rules-catalog.md`, `docs/5-scoring-model.md` | Domain Lead | Accepted | 2026-03-05 |
| 4. API Contract | `docs/api/openapi-v1.yaml`, `docs/interfaces/domain-types-v1.md` | API Lead | Accepted | 2026-03-05 |
| 5. Security/POPIA | `docs/7-security-baseline.md`, `docs/8-popia-basics.md` | Security Lead | Accepted | 2026-03-05 |
| 6. Brand System | `docs/13-brand-system.md`, `docs/design/tokens-v1.json` | Design Lead | Accepted | 2026-03-05 |
| 7. Backlog + Test Plan | `docs/10-mvp-backlog.md`, `docs/11-test-plan.md` | Engineering Lead | Accepted | 2026-03-05 |

## Hard Rule

- If any gate is not `Accepted`, feature coding cannot start.

## Sign-Off Notes

Use this section to capture decisions, exceptions, or temporary waivers with explicit expiry dates.

## Validation Decision Notes (2026-03-05)

1. Scope and positioning are aligned: internal TaxTrack heuristic, not SARS decision output.
2. Data/risk model now reflects reconciliation-first PAYE/VAT logic with evidence completeness.
3. API contracts include async status, policy metadata, idempotency, and privacy request workflows.
4. Security/POPIA baseline includes sections 21, 22, 71, 72 and Information Officer governance.
5. Brand system remains approved and unchanged.
6. Additional engineering controls are now mandatory via:
   - `docs/14-secure-coding-standards.md`
   - `docs/15-ai-assisted-coding-guardrails.md`

## Web Verification Sources (2026-03-05)

1. SARS VAT overview: https://www.sars.gov.za/types-of-tax/value-added-tax/
2. SARS VAT obligations: https://www.sars.gov.za/types-of-tax/value-added-tax/obligations-of-a-vat-vendor/
3. SARS VAT refunds and evidence: https://www.sars.gov.za/types-of-tax/value-added-tax/vat-refunds-for-vendors/
4. SARS tax invoices: https://www.sars.gov.za/businesses-and-employers/government/tax-invoices/
5. SARS PAYE overview: https://www.sars.gov.za/types-of-tax/pay-as-you-earn/
6. SARS PAYE reconciliations: https://www.sars.gov.za/types-of-tax/pay-as-you-earn/reconciliations/
7. SARS VAT rate legal-process release (2025 reversal context): https://www.sars.gov.za/media-release/sars-welcomes-court-order-relating-to-the-vat-rate-originally-announced-to-come-into-effect-on-1-may-2025/
8. SARS published tax rates context: https://www.sars.gov.za/tax-rates/other-taxes/
9. POPIA Act text: https://www.justice.gov.za/legislation/acts/2013-004.pdf
10. Information Regulator POPIA portal/guidance: https://inforegulator.org.za/popia/
