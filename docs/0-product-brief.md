# TaxTrack Product Brief

## One-Sentence Product Definition

TaxTrack is a SaaS tax-risk intelligence platform that analyzes company financial data and warns users about likely compliance risks before SARS audit action.

## Target Users

- SME business owners
- Accountants managing one or more client companies
- Finance managers in growth-stage businesses

## Core Problem

Businesses often discover VAT, payroll, or declaration mistakes only after audit review, resulting in penalties, interest, legal cost, and operational stress.

## Core Promise

Upload financial data and receive a clear risk score, actionable alerts, and a compliance report within seconds.

The risk score is an internal TaxTrack heuristic and not an official SARS score or SARS decision.

## What TaxTrack Is Not

- Not tax advice
- Not legal advice
- Not an auto-filing service to SARS

## Value Proposition

- Early detection before audit escalation
- Consistent, explainable rule-based risk outputs
- Clear remediation recommendations
- Auditable record of uploads and analysis events

## MVP Success Criteria

1. A registered user can create a company profile and upload CSV files without manual intervention.
2. The platform produces a deterministic `TaxRiskScore` from rule evaluations in under 10 seconds for typical SME datasets.
3. Users can view alerts in a dashboard and download a compliance report.
4. Every upload and analysis event is audit logged.

## Primary Constraints

- Rule-based detection only for MVP (no AI/ML)
- CSV upload input only for MVP
- JWT authentication and role-based authorization
- PostgreSQL as the system of record
