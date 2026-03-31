# MVP Wireframes (Low Fidelity)

This document defines text wireframes and required UI states for MVP.

## Screen 1: Register / Login

### Layout

- Left panel: product value statement.
- Right panel: tabbed `Register` and `Login` forms.
- Primary action buttons use `primary` token.

### States

- Empty: untouched form.
- Loading: submit button disabled with spinner.
- Error: inline validation and top-level auth error banner.
- Success: redirect to company setup.

## Screen 2: Company Setup

### Layout

- Form fields: company name, registration number, industry, tax reference.
- Save button in `primary`.
- Secondary cancel/back in `secondary`.

### States

- Empty: all fields blank.
- Loading: create request pending.
- Error: field-specific validation and conflict banner.
- Success: confirmation and navigation to upload.

## Screen 3: CSV Upload

### Layout

- Dataset selector: `company`, `transactions`, `payroll`, `vat_submissions`.
- Drag-drop area + browse button.
- Validation summary card and row-level error table.

### States

- Empty: no file selected.
- Loading: upload progress and queued status.
- Error: parse/validation errors with row, column, code.
- Success: upload accepted card with upload ID.

## Screen 4: Risk Dashboard

### Layout

- Top card: `Tax Risk Score` numeric value and `RiskLevel` badge.
- Cards: rule trigger counts, latest analysis time, company summary.
- Alerts list table: rule code, severity, description, recommendation.

### Color Semantics

- Low risk badge: `success`
- Medium risk badge: `warning`
- High risk badge: `danger`
- Surfaces: `bg` and `card`
- Body text: `text-primary`, helper text: `text-secondary`

### States

- Empty: no analysis yet prompt.
- Loading: skeleton cards.
- Error: retry banner.
- Success: populated score and alerts.

## Screen 5: Report Download

### Layout

- Report summary card with generated timestamp.
- Download options: JSON only for MVP v1.
- History of previous reports.

### States

- Empty: no report generated yet.
- Loading: report generation in progress.
- Error: generation failure with retry.
- Success: authenticated JSON download action with expiry metadata.

## Story To Screen Mapping

| Story IDs | Screen |
|---|---|
| 1, 2 | Register / Login |
| 3, 4 | Company Setup |
| 5, 6, 7, 16 | CSV Upload |
| 8, 9, 10, 11, 12, 17 | Risk Dashboard |
| 13 | Report Download |
| 14, 15, 18 | Cross-screen access and audit flows |

## Wireframe Review Checklist

1. Every screen includes empty/loading/error/success states.
2. Risk state colors use semantic tokens only.
3. No decorative use of warning/danger/success colors.
4. Forms and alerts remain readable at mobile widths.
