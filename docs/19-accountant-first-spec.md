# Accountant-First Product Spec (Phase 2)

## Purpose

Define the accountant-first experience that turns TaxTrack into a portfolio workflow tool. This spec focuses on product behaviors and user journeys, not UI layouts or data models.

## Primary Users And Roles

- Firm Owner: manages portfolio, billing, and team access.
- Accountant: triages alerts, performs reconciliations, tracks remediation.
- Reviewer: verifies that remediation is complete and documented.
- Client Contact: receives summaries and provides missing evidence.

## Core Journeys

1. Onboard firm and create first portfolio
2. Add or invite client companies
3. Upload or ingest client data for a reporting period
4. Run analysis and triage alerts by risk and due date
5. Assign remediation tasks and track progress
6. Mark alerts resolved or accepted risk with notes
7. Generate monthly compliance summaries per client

## Core Features

- Portfolio dashboard with risk sorting and filters
- Company grouping and tagging
- Alert triage states: New, In Review, Needs Client, Resolved, Accepted Risk
- Remediation notes and evidence attachments
- Monthly compliance checklist per company
- Remediation history and audit trail

## Required API Surface (Documented Only)

- Portfolio list and filtering
- Company grouping and tag updates
- Alert triage state updates
- Remediation notes and status changes
- Company-level compliance checklist retrieval

## Success Metrics

- Companies managed per accountant per month
- Alerts triaged per company per month
- Remediation completion rate
- Average time to resolution for Critical and Warning alerts

## Non-Goals For This Phase

- AI-driven alert scoring or summaries
- Deep accounting system integrations
- Full audit management suite
- Enterprise SSO or SCIM

## Assumptions

- TaxTrack remains the source of truth for scoring and alerts.
- Accountants need a repeatable monthly workflow, not just a score dashboard.
- Portfolio management must be faster than spreadsheets to gain adoption.
