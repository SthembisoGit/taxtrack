# Contributing To TaxTrack

## Purpose

This repository is docs-first until the readiness gates are approved. Contributions must reduce ambiguity, improve delivery confidence, or implement approved backlog items once coding starts.

## Branching Strategy

- `main` is always protected.
- All work happens in short-lived branches.

### Branch Naming

- `feature/<ticket-or-topic>-<short-description>`
- `fix/<ticket-or-topic>-<short-description>`
- `docs/<topic>`
- `chore/<topic>`

Examples:

- `feature/TAX-42-upload-validation`
- `docs/risk-rules-catalog`

## Commit Message Convention

Use Conventional Commits:

- `feat: ...`
- `fix: ...`
- `docs: ...`
- `test: ...`
- `refactor: ...`
- `chore: ...`

Example:

`docs: add v1 CSV data contract and validation codes`

## Pull Request Process

1. Rebase branch on latest `main`.
2. Complete the PR template.
3. Link issue/story IDs.
4. Ensure required checks pass.
5. Request review from at least one maintainer.

## Engineering Rules

- Keep clean architecture boundaries (API, Application, Domain, Infrastructure).
- Do not place business logic in controllers.
- Use semantic enums (`RiskLevel`, `AlertSeverity`) across API and UI.
- Do not hardcode UI colors; use `docs/design/tokens-v1.json`.
- Do not add AI/ML logic in MVP risk engine.

## Quality Gates

Every PR must satisfy:

- Scope alignment with `docs/1-scope.md`
- Contract alignment with `docs/3-data-contract.md`
- API alignment with `docs/api/openapi-v1.yaml`
- Security alignment with `docs/7-security-baseline.md`
- Secure coding alignment with `docs/14-secure-coding-standards.md`
- AI coding guardrails alignment with `docs/15-ai-assisted-coding-guardrails.md`
- Readiness gate updates where applicable

## Definition Of Done

A change is done only when:

1. Acceptance criteria are met.
2. Tests are added or updated.
3. Documentation is updated.
4. No unresolved critical comments remain.
