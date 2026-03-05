# TaxTrack Architecture v1

## Overview

TaxTrack follows clean architecture with strict separation of concerns:

1. API Layer
2. Application Layer
3. Domain Layer
4. Infrastructure Layer

## Logical Component Diagram

```text
React Frontend
    |
    v
ASP.NET Core API (Controllers, Auth Middleware)
    |
    v
Application Services (Use Cases, DTO Mapping, Validation)
    |
    v
Domain (Entities, Rule Engine Interfaces, Business Policies)
    |
    v
Infrastructure (EF Core Repositories, PostgreSQL, File Storage, Queue Worker)
```

## Layer Responsibilities

### API Layer

- Exposes REST endpoints.
- Applies authentication and authorization.
- Converts transport models to application commands.
- Returns RFC 7807 error payloads.

### Application Layer

- Coordinates use cases (`RegisterUser`, `UploadFinancialData`, `AnalyzeRisk`).
- Enforces workflow rules and idempotency behavior.
- Handles transaction boundaries.
- Publishes domain events for audit logging.

### Domain Layer

- Defines entities and value objects.
- Owns risk rule definitions and scoring policies.
- Contains no framework or database dependencies.

### Infrastructure Layer

- Implements repositories with PostgreSQL + EF Core.
- Persists audit logs and analysis results.
- Executes background jobs for upload parsing and risk analysis.
- Handles report artifact storage and retrieval.

## Data Flow: Upload To Score

1. User uploads CSV via `POST /api/financial/upload` with idempotency key.
2. API stores file metadata and queues validation job.
3. Worker validates against `docs/3-data-contract.md`.
4. Valid rows are persisted in normalized tables.
5. User triggers `POST /api/risk/analyze`.
6. Worker evaluates rules from `docs/4-risk-rules-catalog.md`.
7. Score and alerts are persisted as `TaxRiskResult` and `RiskAlert`.
8. API returns latest result through `GET /api/risk/{companyId}`.
9. Report endpoint generates report payload and download metadata.

## Multi-Tenancy Model

- Tenant boundary is the `Company`.
- Access is enforced by `CompanyMembership` records.
- A user may belong to multiple companies with role per membership.
- Data queries must include company scope at repository or query policy level.

## Background Processing Model

- Upload and analysis are asynchronous operations.
- API returns `202 Accepted` for queued operations.
- Jobs are idempotent by key + payload fingerprint.
- Status transitions: `Queued -> Processing -> Completed|Failed`.

## Non-Functional Targets (MVP)

- Typical analysis response availability: under 10 seconds for SME-sized datasets.
- Audit log write for all sensitive actions.
- 99.5 percent monthly API availability target on bootstrap hosting.
- Deterministic rule results for identical inputs.

## Deployment Topology (MVP)

- Frontend: Vercel
- Backend API: Railway or Render
- Database: Neon PostgreSQL

## Evolution Path

- Move backend to Azure App Service.
- Move database to Azure Database for PostgreSQL.
- Add Azure Blob for report and upload storage.
- Add Azure Application Insights for observability.
