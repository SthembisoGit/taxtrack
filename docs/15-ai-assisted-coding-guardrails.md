# TaxTrack AI-Assisted Coding Guardrails v1

Verified against published research and standards on 2026-03-05.

## Common AI Coding Failure Modes

1. Insecure code suggestions accepted without threat modeling or validation.
2. Hallucinated APIs, libraries, or configuration flags.
3. Missing authorization checks on multi-tenant data paths.
4. Weak error handling that leaks internals or swallows failures.
5. Broken edge-case logic (time ordering, boundary values, idempotency replay).
6. Contract drift between code, OpenAPI, and documentation.
7. Hardcoded secrets or unsafe default credentials in examples.

## Required Mitigations

1. Verify every security-sensitive claim against official docs/specs before merge.
2. Require tests for boundary values (`40/41/70/71`), idempotency replay, and tenant isolation.
3. Review AI-generated code for CWE Top 25 patterns before approval.
4. Reject code with unknown APIs or unverifiable snippets.
5. Keep changes small and traceable: code + tests + docs in the same PR.
6. For legal/policy logic, record source URL and effective date in docs.
7. Require at least one human reviewer for auth, cryptography, privacy, and risk-engine changes.

## Review Checklist (Mandatory)

1. Does the code match the contract (`docs/api/openapi-v1.yaml`)?
2. Does the code preserve clean architecture boundaries?
3. Are security controls from `docs/7-security-baseline.md` satisfied?
4. Are POPIA controls from `docs/8-popia-basics.md` satisfied?
5. Are comments minimal, accurate, and non-redundant?

## References

1. Pearce et al. (2021), "Asleep at the Keyboard? Assessing the Security of GitHub Copilot's Code Contributions": https://arxiv.org/abs/2108.09293
2. Sandoval et al. (2022), "Do Users Write More Insecure Code with AI Assistants?": https://arxiv.org/abs/2211.03622
3. MITRE CWE Top 25 (for review patterns): https://cwe.mitre.org/top25/
