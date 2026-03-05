# ADR-002: PostgreSQL With EF Core

## Status

Accepted

## Context

TaxTrack requires relational integrity, transactional consistency, and strong support for reporting and query patterns.

## Decision

Use PostgreSQL as primary data store and EF Core as ORM for persistence mapping and migrations.

## Consequences

- Strong relational modeling for companies, memberships, and risk outputs.
- Mature .NET tooling and migration workflow.
- Requires careful migration discipline in CI/CD.

## Alternatives Considered

- MongoDB (rejected: less natural fit for relational authorization and reporting joins)
- Dapper only (rejected: more manual mapping overhead for MVP team velocity)
