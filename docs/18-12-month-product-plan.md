# 12-Month Product Plan (Advanced Roadmap Execution)

## Summary

This is a 12-month execution plan derived from the advanced roadmap. It assumes a steady, realistic delivery pace and prioritizes trust and workflow depth before integrations and intelligence features.

## Resource Assumptions

- Baseline: solo builder or 1-2 engineers.
- Stretch: small team of 3-4 (backend, frontend, product).

## Dependencies

- Phase 1 hardening must be completed before Phase 2 workflow depth.
- Phase 2 workflow depth must land before connected monitoring in Phase 3.
- Phase 3 monitoring should precede benchmarking and intelligence features.

## Quarter 1 (Months 1-3): Phase 1 Hardening And Pilot Stability

In scope:

- transactional uploads and rollback safety
- duplicate protection across imports
- EF Core migration workflow and startup hardening
- production CORS configuration
- observability basics (logs, correlation IDs, health probes)
- backup and restore runbook
- clear error and recovery states in the UI

Not in scope yet:

- integrations (Xero, Sage, cloud storage)
- accountant workflow tooling beyond basics
- AI or benchmarking

## Quarter 2 (Months 4-6): Phase 2 Workflow Depth (Accountant First)

In scope:

- accountant portfolio dashboard
- company grouping and filtering
- alert triage states and remediation tracking
- notes and evidence attachments
- monthly compliance checklist per company

Not in scope yet:

- automated ingestion
- benchmark comparisons
- n8n integration

## Quarter 3 (Months 7-9): Phase 3 Connected Monitoring Foundations

In scope:

- recurring ingestion from cloud file storage
- scheduled re-analysis
- notification routing by severity
- first automation workflows (email and reminders)

Not in scope yet:

- deep accounting API integrations
- benchmarking layer
- AI summary layer

## Quarter 4 (Months 10-12): Phase 4 Intelligence Layer (Foundations)

In scope:

- trend history views
- score-change explanations
- early benchmarking design with privacy safeguards
- what-changed summaries for accountants

Not in scope yet:

- enterprise SSO and SCIM
- advanced fraud detection
- policy packs for other jurisdictions

## Delivery Notes

- The plan assumes incremental shipping each quarter, not a single large release.
- Integrations are delayed until the core system is trusted and stable.
- Each quarter should include a regression stabilization period before expanding scope.
