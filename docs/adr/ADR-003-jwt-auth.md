# ADR-003: JWT Authentication With Refresh Tokens

## Status

Accepted

## Context

The platform requires stateless API authentication for web clients and secure session continuity.

## Decision

Use JWT bearer access tokens and rotating refresh tokens.

- Access token lifetime: 15 minutes
- Refresh token lifetime: 7 days
- Password hashing: BCrypt

## Consequences

- Scales well for stateless APIs.
- Requires secure refresh token storage and revocation strategy.
- Requires strict key rotation and token validation controls.

## Alternatives Considered

- Cookie session auth (rejected for API-first architecture)
- Third-party identity provider in MVP (deferred to scale phase)
