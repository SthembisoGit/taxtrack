# Phase 1 Hardening Checklist (Pilot-Grade Readiness)

## Purpose

This checklist translates Phase 1 goals into concrete, testable hardening tasks. It is the minimum bar before onboarding pilot customers.

## Upload Integrity And Data Safety

- [ ] Wrap CSV parsing and record persistence in a single transaction so failed validation does not write partial rows.
- [ ] Ensure a failed upload leaves no persisted data for that upload job.
- [ ] Persist upload summaries only after the full file passes validation.
- [ ] Include a clear failure reason and validation issues for every failed upload.

## Duplicate Protection And Idempotency

- [ ] Prevent duplicate records across uploads for the same company and dataset type.
- [ ] Define a de-duplication rule using (companyId + datasetType + source_record_id) or equivalent.
- [ ] Enforce the rule at both application and database levels.
- [ ] Keep current idempotency key behavior for request-level replays.

## Migration And Startup Hardening

- [ ] Replace Database.EnsureCreatedAsync() with EF Core migrations.
- [ ] Add a documented migration workflow for local and production environments.
- [ ] Ensure the API fails fast if migrations are missing or invalid in production.

## Production CORS Configuration

- [ ] Add CORS policy with explicit allowlist for production frontend origins.
- [ ] Support environment-based configuration of allowed origins.
- [ ] Keep local dev permissive only for localhost during development.

## Observability And Diagnostics

- [ ] Ensure structured logs include correlation IDs for upload and analysis flows.
- [ ] Confirm health endpoints are deployed and monitored.
- [ ] Add minimum error monitoring: unhandled exceptions, upload failures, analysis failures.

## Backup And Restore

- [x] Document backup schedule for the database.
- [x] Document a restore procedure and a basic disaster recovery checklist.
- [ ] Verify that backups include all audit logs and risk results.

## UI Reliability And Clarity

- [ ] Ensure every error state has a clear, actionable message.
- [ ] Ensure upload and analysis states are consistent across dashboard and report.
- [ ] Verify that the user can recover without support in common failure cases.

## Exit Criteria (Aligned To docs/12-readiness-checklist.md)

- [ ] Uploads are transactional and do not partially persist on failure.
- [ ] Duplicate protection is enforced and tested.
- [ ] Migrations are the default startup path for production.
- [ ] CORS is configurable and locked down for production.
- [ ] Health probes and logging are live and verified.
- [ ] Backup and restore steps are documented and tested at least once.
- [ ] Pilot users can complete end-to-end flows without developer intervention.
