# ADR-004: Bootstrap Hosting Stack

## Status

Accepted

## Context

MVP requires low-cost hosting with HTTPS and straightforward deployment pipelines.

## Decision

Use:

- Backend API on Railway or Render
- PostgreSQL on Neon
- Frontend on Vercel

Define migration path to Azure services after MVP validation.

## Consequences

- Fast startup and low operational overhead.
- Possible provider limits in free tiers.
- Migration planning required once traffic or compliance needs grow.

## Alternatives Considered

- Azure-first from day one (rejected for MVP cost/complexity)
- Self-hosted VPS (rejected for operational burden)
