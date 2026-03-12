# TaxTrack Advanced Roadmap

## Purpose

This document defines the phased evolution of TaxTrack beyond MVP, using the strongest market position available:

**TaxTrack is a South Africa-first tax and compliance intelligence layer for SMEs, finance teams, and accountants.**

It is not positioned as a replacement for Sage, Xero, SimplePay, GreatSoft, or other accounting and filing systems.  
Its advantage is pre-submission risk detection, reconciliation intelligence, explainable alerts, and remediation workflows.

## Strategic Positioning

### Core Wedge

TaxTrack should win on:

1. pre-audit risk detection before SARS pressure
2. reconciliation across accounting, payroll, and VAT datasets
3. explainable rule-driven alerts with evidence
4. accountant and finance-team workflow support

### What TaxTrack Should Not Try To Be

TaxTrack should not lead with:

1. general bookkeeping
2. payroll processing
3. tax return filing as the primary product
4. generic AI tax advice
5. a full ERP replacement

### Best Customer Entry Point

The recommended customer order is:

1. accountants and tax practitioners with multiple SME clients
2. finance managers in small and mid-sized companies
3. owners of compliance-sensitive SMEs

This sequence gives TaxTrack the best distribution and retention path because accountants can bring multiple companies onto the platform.

## Roadmap Principles

1. Core scoring, validation, policy logic, tenant access, and audit logging remain in the .NET platform.
2. Automation and integration orchestration may later be added through `n8n`, but only around the platform, never as the source of truth.
3. Every phase must improve trust, not just feature count.
4. False positives and duplicated data must be reduced before advanced intelligence features are introduced.
5. Each phase should create a stronger reason for customers to stay in the product every month.

## System Boundary Rules

### Must Stay In TaxTrack Core

The following remain inside the .NET backend and database:

1. authentication and authorization
2. tenant isolation
3. CSV and integration data validation
4. rule engine and scoring
5. tax policy versioning
6. evidence completeness logic
7. audit logs
8. privacy workflow state
9. report integrity and consistency rules

### Can Be Added Through n8n Later

The following are good candidates for `n8n` once the platform grows:

1. email, Slack, and Teams notifications
2. scheduled reminders and digest jobs
3. file pickup from cloud storage
4. CRM/helpdesk/task system integration
5. internal operational alerts
6. onboarding and support automations

## Phase 1: Pilot-Grade Compliance Intelligence

### Goal

Make TaxTrack trusted enough for controlled real-world use by a small number of firms and companies.

### Product Promise

"Upload real finance data and get a deterministic, explainable tax risk view before filing."

### Required Product Capabilities

1. stable upload and analysis flow
2. no duplicate double-counting from repeated uploads
3. transactional upload processing so failed uploads do not partially affect future analysis
4. clearer report and dashboard consistency
5. evidence completeness surfaced as a confidence signal
6. stronger empty, loading, retry, and error states

### Required Technical Work

1. upload transaction boundaries and rollback safety
2. duplicate-record protection across imports
3. EF Core migration workflow and startup hardening
4. production CORS and deployment configuration
5. observability basics: logs, request correlation, health probes, deployment smoke checks
6. data backup and restore procedure

### Commercial Objective

Run pilots with 3-10 customers and prove that users trust the alerts enough to review them before filing.

### Exit Criteria

1. pilot customers can complete the end-to-end flow without developer support
2. duplicate upload risk is controlled
3. upload failure cannot corrupt analysis state
4. support issues are mostly product issues, not infrastructure failures

## Phase 2: Accountant Workflow And Multi-Company Operations

### Goal

Turn TaxTrack from a single-company analyzer into a working tool for accountants and portfolio managers.

### Product Promise

"Manage many companies, prioritize the riskiest ones, and track remediation work in one place."

### Key Features

1. accountant portfolio dashboard
2. company grouping and filtering
3. alert triage workflow
4. remediation status per alert
5. notes, evidence attachments, and resolution history
6. role refinement for owners, reviewers, and firm staff
7. monthly compliance checklist per company

### Data And Logic Enhancements

1. company-level trend history
2. repeated anomaly streak tracking
3. rule suppression with audit reason
4. manual override or review status without changing the raw score

### Commercial Objective

Make accounting firms the main acquisition channel and increase revenue per customer through multi-company usage.

### Exit Criteria

1. an accountant can manage 20+ companies efficiently from one workspace
2. users can move alerts from "detected" to "resolved" inside TaxTrack
3. TaxTrack becomes part of the monthly compliance routine

## Phase 3: Connected Monitoring And Automation

### Goal

Reduce manual upload friction and make TaxTrack part of regular operational workflows.

### Product Promise

"TaxTrack monitors connected finance data continuously and warns you before compliance issues pile up."

### Key Features

1. recurring ingestion from accounting and payroll systems
2. scheduled re-analysis
3. near-real-time risk change alerts
4. uploadless monitoring for connected tenants
5. notification routing by user role and severity

### Integration Priorities

1. cloud file ingestion first
2. accounting exports second
3. API-based integrations later

Recommended order:

1. Google Drive / OneDrive / Dropbox pickup
2. Xero exports
3. Sage exports
4. payroll-system feeds

### n8n Introduction Point

`n8n` becomes useful in this phase.

Suggested responsibilities:

1. poll connected storage folders
2. trigger file import jobs
3. send email/Slack/Teams notifications
4. send weekly unresolved-risk digests
5. create follow-up tasks in ticketing systems

### Still Not In n8n

These stay in TaxTrack:

1. validation logic
2. score calculation
3. rule evaluation
4. policy versioning
5. company access control
6. audit logging

### Commercial Objective

Increase stickiness by reducing manual work and turning TaxTrack into a recurring monitoring system.

### Exit Criteria

1. at least one connected ingestion path is reliable
2. risk notifications drive repeat logins and remediation activity
3. monthly upload dependency is reduced for pilot accounts

## Phase 4: Benchmarking And Intelligence Layer

### Goal

Move from "you have risk" to "here is how you compare and what is changing."

### Product Promise

"See how your tax posture compares with peers and where risk is trending."

### Key Features

1. industry benchmarking
2. company size-band comparisons
3. trend charts by tax area
4. recurring anomaly heatmaps
5. score-change explanations
6. what-changed summaries between analysis runs

### Data Requirements

1. normalized anonymized benchmark pools
2. industry classification maturity
3. stronger historical data storage
4. benchmark governance and privacy protections

### Commercial Objective

Make the product feel more valuable than a rule checklist by adding context that firms and finance teams cannot easily build themselves.

### Exit Criteria

1. benchmark outputs are statistically meaningful and privacy-safe
2. users can explain risk changes without opening raw records every time
3. customer retention improves because the product becomes more decision-support oriented

## Phase 5: Compliance Operations Platform

### Goal

Expand beyond scoring into a system that helps companies run a repeatable compliance process.

### Product Promise

"TaxTrack helps teams detect, investigate, assign, resolve, and evidence compliance issues."

### Key Features

1. case management for alerts
2. approval workflows for high-severity issues
3. monthly and quarterly work queues
4. board-ready and audit-ready reporting packs
5. document repository linked to alerts and filing cycles
6. service-level tracking for remediation
7. richer privacy and breach workflow operations

### Operational Integrations

1. ticketing systems
2. document storage
3. email and collaboration tools
4. internal workflow systems

### Commercial Objective

Compete on workflow depth and operational fit rather than only on the scoring engine.

### Exit Criteria

1. customers use TaxTrack to run compliance work, not only view scores
2. accountants can coordinate teams and clients inside the platform
3. generated reports are used in real review meetings

## Phase 6: Enterprise And Ecosystem Expansion

### Goal

Support larger organizations and regulated buyers with stronger governance, controls, and integration depth.

### Product Promise

"TaxTrack fits securely into enterprise compliance environments without sacrificing explainability."

### Key Features

1. SSO and SCIM
2. fine-grained role and approval models
3. data residency and retention controls
4. enterprise audit export
5. advanced API integration program
6. enterprise-grade observability and support tooling
7. policy packs and jurisdiction expansion foundations

### Architecture Enhancements

1. dedicated job infrastructure
2. stronger storage boundaries
3. analytics warehouse or benchmark data layer
4. tenant-level configuration and policy packs
5. enterprise monitoring and incident workflows

### Commercial Objective

Open the path to larger finance teams, group structures, and higher-value accounting firms.

### Exit Criteria

1. enterprise onboarding does not require code changes per tenant
2. security and audit expectations match procurement requirements
3. integration and governance features support larger contract sizes

## AI Introduction Policy

AI should be introduced only after Phases 1-3 are strong.

### Good AI Uses Later

1. summarizing triggered alerts in plain language
2. suggesting investigation steps
3. classifying likely root causes
4. highlighting important narrative changes in reports

### Bad AI Uses Too Early

1. replacing deterministic scoring
2. overriding statutory logic
3. making final compliance judgments without evidence

## Success Metrics By Stage

### Phase 1

1. upload success rate
2. analysis completion rate
3. support tickets per active company
4. percentage of alerts with accepted evidence completeness

### Phase 2

1. companies per accountant tenant
2. alerts reviewed per month
3. remediation completion rate

### Phase 3

1. repeat logins per month
2. connected-company percentage
3. notification-to-action rate

### Phase 4

1. benchmark feature usage
2. retention uplift for firms
3. reduction in unexplained score changes

### Phase 5-6

1. workflow completion time
2. enterprise win rate
3. expansion revenue per tenant

## Recommended Build Order From Current State

Starting from the current codebase, the recommended next order is:

1. phase 1 hardening gaps
2. accountant multi-company workflow depth
3. connected monitoring foundations
4. `n8n` notification and scheduling layer
5. benchmarking and trend intelligence
6. enterprise and ecosystem expansion

## Immediate Recommendation

TaxTrack should spend its next major cycle on **trust and workflow depth**, not on AI or broad integrations.

The most valuable next milestone is:

**"Pilot-grade compliance intelligence for accountants and SMEs."**

That means:

1. reliable uploads
2. duplicate protection
3. remediation workflow basics
4. stronger accountant portfolio support
5. operational trust

Only after that should TaxTrack expand into automation-heavy monitoring and intelligence layers.
