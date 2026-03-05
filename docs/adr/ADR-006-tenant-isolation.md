# ADR-006: Company-Scoped Tenant Isolation

## Status

Accepted

## Context

TaxTrack must support single-company users and accountants managing multiple client companies without cross-tenant data leakage.

## Decision

Use company-scoped tenancy with explicit `CompanyMembership` assignments.

- Every protected operation must resolve actor membership for the target company.
- Every query for tenant-bound resources must include company scope.
- Access outside membership returns `403 Forbidden`.

## Consequences

- Supports core accountant use case without enterprise complexity.
- Requires consistent authorization checks across endpoints and jobs.
- Enables future expansion to organization-level tenancy if required.

## Alternatives Considered

- Single-company tenancy only (rejected: excludes accountant segment)
- Full enterprise multi-org hierarchy in MVP (rejected: unnecessary complexity)
