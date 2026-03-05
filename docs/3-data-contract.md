# TaxTrack Data Contract v1

## Purpose

This document defines the canonical CSV contract for MVP uploads. Validation must be deterministic and versioned.

## Global Rules

1. Encoding: UTF-8.
2. Delimiter: comma.
3. Header row required.
4. All files must include `contract_version` column with value `v1`.
5. Date format: `YYYY-MM-DD`.
6. Decimal format: dot decimal, up to 2 decimal places.
7. Currency default: `ZAR` when omitted only in fields marked optional.
8. Empty strings are treated as null for optional columns, invalid for required columns.
9. File size limit for MVP: 10 MB per upload.
10. Maximum rows for MVP: 100,000 rows per file.
11. Evidence fields required for statutory reconciliation checks must be present and valid.
12. Contract validation must emit `evidenceCompleteness` metadata for uploaded datasets.

## Dataset Types

- `company`
- `transactions`
- `payroll`
- `vat_submissions`

## Contract: Company File (`company`)

### Required Columns

| Column | Type | Rule |
|---|---|---|
| contract_version | string | must equal `v1` |
| company_name | string | 2 to 150 chars |
| registration_number | string | unique per company |
| industry | enum | see allowed values |
| tax_reference | string | 10 to 15 chars |
| country_code | string | ISO alpha-2, must be `ZA` for MVP |
| currency | string | ISO 4217, default `ZAR` |
| financial_year_start | date | valid ISO date |
| created_at | date | valid ISO date |

### Allowed `industry` Values

- Manufacturing
- Retail
- Logistics
- ProfessionalServices
- Technology
- Construction
- Hospitality
- Other

## Contract: Transactions File (`transactions`)

### Required Columns

| Column | Type | Rule |
|---|---|---|
| contract_version | string | must equal `v1` |
| source_record_id | string | unique within file |
| company_registration_number | string | must match existing company |
| transaction_date | date | valid ISO date |
| ledger_category | enum | see allowed values |
| description | string | 1 to 255 chars |
| amount | decimal(18,2) | absolute amount > 0 |
| currency | string | ISO 4217, default `ZAR` |
| vat_amount | decimal(18,2) | >= 0 |
| direction | enum | `debit` or `credit` |
| source_system | string | 1 to 80 chars |
| tax_invoice_number | string | required for VATInput rows |
| supplier_vat_number | string | required for VATInput rows |
| tax_invoice_date | date | required for VATInput rows |
| vat201_reference | string | optional for non-VAT lines, required where linked to a VAT period |

### Allowed `ledger_category` Values

- Revenue
- CostOfSales
- Salaries
- PAYE
- UIF
- SDL
- VATInput
- VATOutput
- Entertainment
- Travel
- ProfessionalFees
- Rent
- Utilities
- Other

## Contract: Payroll File (`payroll`)

### Required Columns

| Column | Type | Rule |
|---|---|---|
| contract_version | string | must equal `v1` |
| source_record_id | string | unique within file |
| company_registration_number | string | must match existing company |
| pay_period | date | first day of pay month |
| gross_salary | decimal(18,2) | > 0 |
| total_paye | decimal(18,2) | >= 0 |
| employee_count | integer | >= 1 |
| total_uif | decimal(18,2) | >= 0 |
| total_sdl | decimal(18,2) | >= 0 |
| currency | string | ISO 4217, default `ZAR` |
| emp201_period | date | first day of EMP201 month |
| emp201_declared_paye | decimal(18,2) | >= 0 |
| emp201_declared_uif | decimal(18,2) | >= 0 |
| emp201_declared_sdl | decimal(18,2) | >= 0 |
| emp501_reference | string | required for reconciliation cycles |
| irp5_total_paye | decimal(18,2) | >= 0 |
| submission_date | date | valid ISO date |
| payment_date | date | valid ISO date and >= pay_period |
| payment_reference | string | 1 to 50 chars |

## Contract: VAT Submissions File (`vat_submissions`)

### Required Columns

| Column | Type | Rule |
|---|---|---|
| contract_version | string | must equal `v1` |
| source_record_id | string | unique within file |
| company_registration_number | string | must match existing company |
| tax_period_start | date | valid ISO date |
| tax_period_end | date | valid ISO date and >= start |
| output_vat | decimal(18,2) | >= 0 |
| input_vat | decimal(18,2) | >= 0 |
| vat_refund_claimed | decimal(18,2) | >= 0 |
| declared_turnover | decimal(18,2) | > 0 |
| currency | string | ISO 4217, default `ZAR` |
| submission_date | date | valid ISO date |
| vat201_reference | string | required return reference |
| payment_date | date | valid ISO date and >= tax_period_end |
| payment_reference | string | 1 to 50 chars |

## Validation Error Model

Validation errors must include:

- `rowNumber`
- `columnName`
- `errorCode`
- `message`

### Error Codes

| Code | Meaning |
|---|---|
| `DC001` | Missing required column |
| `DC002` | Unsupported contract version |
| `DC003` | Invalid date format |
| `DC004` | Invalid decimal format |
| `DC005` | Enum value not allowed |
| `DC006` | Required value missing |
| `DC007` | Value out of allowed range |
| `DC008` | Duplicate `source_record_id` in file |
| `DC009` | Unknown company registration number |
| `DC010` | Cross-field validation failed |
| `DC011` | Missing statutory evidence field |
| `DC012` | Reconciliation key mismatch |
| `DC013` | Invalid filing or payment timeline |

## Deterministic Processing Rules

1. Schema validation runs before business validation.
2. Processing halts only when file-level contract validation fails.
3. Row-level errors are collected up to 1000 rows for response payload.
4. Validation result for identical file bytes must be identical.
5. Evidence completeness is calculated as a percentage of required statutory evidence fields populated and valid.
6. If evidence completeness is below 80 percent for a statutory dataset, flag `insufficientEvidence=true`.

## Versioning Policy

- Current version: `v1`
- New versions are additive where possible.
- Breaking changes require `v2` and separate sample files.
