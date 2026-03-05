# MVP Backlog And Delivery Plan (8-10 Weeks)

## Sprint 1 (Weeks 1-2): Auth, Company, Roles

### Goals

- User registration and login
- JWT + refresh token issuance
- Company creation and membership model

### Backlog Items

1. Implement auth endpoints contract.
2. Implement password hashing and token policy.
3. Implement company create/get endpoints.
4. Implement company membership authorization checks.

### Exit Criteria

- Auth stories 1-4 pass acceptance criteria.
- Security checks for token auth and role checks pass.

## Sprint 2 (Weeks 3-4): Upload, Validation, Storage

### Goals

- CSV upload endpoint and asynchronous validation
- Contract-enforced parsing and deterministic error responses
- Persist normalized upload records

### Backlog Items

1. Implement upload endpoint with idempotency key.
2. Build CSV parser for all v1 dataset types.
3. Implement validation error model and error codes.
4. Store upload metadata and parsed records.

### Exit Criteria

- Upload stories 5-7 and 16 pass.
- Valid and invalid sample files produce expected outcomes.

## Sprint 3 (Weeks 5-6): Risk Engine And Scoring

### Goals

- Deterministic rule evaluation
- Score computation and risk level mapping
- Alert generation with evidence

### Backlog Items

1. Implement rule interfaces and rule evaluator.
2. Implement v1 rule set from catalog.
3. Implement dual-component scoring (regulatory + heuristic).
4. Implement tax policy versioning with effective-date selection.
5. Persist risk results and alerts with policy and evidence metadata.

### Exit Criteria

- Stories 8-12 pass.
- Boundary tests for 40/41/70/71 pass.

## Sprint 4 (Weeks 7-8): Dashboard And Reports

### Goals

- Dashboard API and frontend display
- Alert visualization by semantic levels
- Report generation and download metadata

### Backlog Items

1. Implement latest risk result endpoint.
2. Implement report endpoint contract.
3. Implement dashboard UI states and token mapping.
4. Add consistency checks for report vs dashboard.
5. Implement async job status endpoints for upload and analysis.

### Exit Criteria

- Stories 13 and 17 pass.
- UI semantic color tests and accessibility checks pass.

## Sprint 5 (Weeks 9-10): Hardening And Pilot Readiness

### Goals

- Security hardening and audit readiness
- POPIA request workflows
- Operational readiness for pilot onboarding

### Backlog Items

1. Implement complete audit event coverage.
2. Add export/delete request recording workflow.
3. Implement POPIA section 21/22/71/72 operational controls.
4. Run regression suite (functional + security + contract + policy-date).
5. Produce pilot runbook and known limitations list.

### Exit Criteria

- Stories 14, 15, 18 pass.
- No unresolved critical defects.
- Readiness checklist fully signed off.
