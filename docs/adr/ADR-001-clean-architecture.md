# ADR-001: Clean Architecture Boundaries

## Status

Accepted

## Context

TaxTrack needs clear boundaries to avoid business logic leaking into controllers or persistence code. The team also needs testable core logic for risk rules.

## Decision

Use clean architecture with four layers:

1. API
2. Application
3. Domain
4. Infrastructure

Dependencies flow inward only. Domain remains framework-agnostic.

## Consequences

- Improved maintainability and testability.
- Slightly higher initial setup effort.
- Clear ownership of business rules in Domain/Application layers.

## Alternatives Considered

- Layered monolith without strict boundaries (rejected: higher long-term coupling)
- Microservices from day one (rejected: too much MVP overhead)
