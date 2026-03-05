# POPIA Basics v1

## Purpose

Define minimum privacy and data-handling controls for South Africa POPIA-aligned operations in MVP.

## Personal Information Processed

- User identity data (email, role, auth metadata)
- Company identifiers and financial records
- Audit logs and access metadata

## Lawful Processing Principles Applied

1. Accountability: documented ownership for security and privacy controls.
2. Processing limitation: collect only fields required for risk analysis.
3. Purpose specification: use data for compliance risk analytics only.
4. Further processing limitation: no unrelated reuse without legal basis.
5. Information quality: validation and correction workflow.
6. Openness: clear privacy notice and processing rationale.
7. Security safeguards: technical and organizational controls.
8. Data subject participation: export and deletion request pathways.

## Data Retention Policy (MVP)

- User account profile data: retained while account is active.
- Financial uploads and analysis records: minimum 5 years where tax law requires record retention; extend for active audit/dispute/legal hold.
- Audit logs: minimum 5 years for tax-compliance traceability, extended where legal hold applies.
- Failed uploads with no legal hold: purge after 90 days.

## Data Subject Rights Workflow

### Export Request

1. Verify requester identity.
2. Log request event.
3. Provide data export package in common format.
4. Mark request as completed and log response timestamp.

### Deletion Request

1. Verify requester identity and authorization.
2. Validate legal retention constraints.
3. If deletable, anonymize or delete data by policy.
4. Log decision and action outcome.

## Third-Party Processors

- POPIA section 21 operator contracts are mandatory for all processors.
- Hosting providers must support encrypted transport and secure secret storage.
- Processor agreements must define breach notification obligations and evidence preservation duties.

## Information Officer Governance

1. Register Information Officer (and deputies where applicable) with the Information Regulator.
2. Maintain an internal POPIA responsibility register and annual review cycle.
3. Track policy approvals, training evidence, and incident-response ownership.

## Breach Notification Baseline

1. Detect and classify incident.
2. Contain and preserve evidence.
3. Notify internal stakeholders immediately.
4. POPIA section 22: notify Information Regulator and affected data subjects as soon as reasonably possible where required.
5. Use the Information Regulator POPIA security compromise process/portal and retain evidence of submission.
6. Track corrective actions to closure.

## Prior Authorisation Screening

1. Screen processing features against POPIA sections 57/58 before release.
2. Escalate to legal review where prior authorisation conditions may apply.
3. Log decision outcome and review date in governance records.

## Cross-Border Transfer Controls (POPIA Section 72)

1. Maintain transfer register for all personal information transfers outside South Africa.
2. Allow transfer only where section 72 lawful basis exists.
3. Require contractual safeguards and security controls from foreign recipients.

## Automated Decision Safeguards (POPIA Section 71)

1. Risk score outputs with substantial effect must include explanation metadata.
2. Provide user path to request human review or submit representation.
3. Record challenge outcomes in audit logs.
