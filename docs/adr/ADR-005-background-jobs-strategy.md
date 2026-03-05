# ADR-005: Asynchronous Background Jobs For Upload And Analysis

## Status

Accepted

## Context

CSV validation and risk analysis may be time-consuming for larger files and should not block API request threads.

## Decision

Use asynchronous job processing for:

- CSV parse and validation
- persistence of normalized records
- risk rule evaluation

API endpoints return `202 Accepted` and expose status tracking identifiers.

## Consequences

- Better responsiveness and resilience for long-running tasks.
- Requires queue infrastructure and retry policies.
- Requires idempotency enforcement to avoid duplicate jobs.

## Alternatives Considered

- Synchronous request processing (rejected: latency and timeout risk)
- Event-driven distributed architecture (deferred for post-MVP scale)
