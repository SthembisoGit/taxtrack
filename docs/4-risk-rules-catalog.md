# TaxTrack Risk Rules Catalog v1

## Purpose

Define deterministic, explainable rules for MVP tax-risk scoring.

Regulatory rules represent statutory reconciliation and filing obligations. Heuristic rules represent internal anomaly indicators and are not statutory SARS thresholds.

## Rule Evaluation Standard

Each rule must specify:

1. Rule code and name
2. Rule class (`Regulatory` or `Heuristic`)
3. Detection purpose
4. Trigger formula
5. Required inputs
6. Weight contribution
7. Alert severity
8. Alert message template
9. False-positive controls
10. Why the rule matters

## Rule Set

## Regulatory Rules

### Rule PAYE_REG_001 - Payroll vs EMP201 PAYE Reconciliation Mismatch

- Rule class: `Regulatory`.
- Detects: material mismatch between payroll-calculated PAYE and EMP201 declared PAYE.
- Trigger formula: `abs(total_paye - emp201_declared_paye) / max(total_paye,1) > 0.05`.
- Required inputs: `total_paye`, `emp201_declared_paye`.
- Weight: `+30`.
- Severity: `Critical`.
- Alert message: "Declared EMP201 PAYE does not reconcile with payroll-calculated PAYE."
- False-positive controls: allow correction window for authorized back-pay adjustments.
- Why this matters: unresolved PAYE mismatches are a major payroll compliance exposure.

### Rule PAYE_REG_002 - EMP201/EMP501/IRP5 Consistency Failure

- Rule class: `Regulatory`.
- Detects: inconsistency between monthly EMP201 totals, reconciliation EMP501 totals, and IRP5 totals.
- Trigger formula: `abs(sum(emp201_declared_paye_for_cycle) - irp5_total_paye) / max(irp5_total_paye,1) > 0.01`.
- Required inputs: `emp201_declared_paye`, `emp501_reference`, `irp5_total_paye`.
- Weight: `+25`.
- Severity: `Critical`.
- Alert message: "EMP201, EMP501, and IRP5 PAYE totals do not reconcile."
- False-positive controls: allow known year-end correction entries with approved reconciliation note.
- Why this matters: reconciliation inconsistencies are likely to trigger verification follow-up.

### Rule PAYE_REG_003 - Filing or Payment Timeliness Breach

- Rule class: `Regulatory`.
- Detects: EMP201 filing or payment submitted after due-date policy.
- Trigger formula: `submission_date > due_date OR payment_date > due_date`.
- Required inputs: `emp201_period`, `submission_date`, `payment_date`, policy calendar (`emp201DueDayOfMonth`, business-day fallback, configured public holidays).
- Weight: `+15`.
- Severity: `Warning`.
- Alert message: "PAYE filing or payment appears late for the declared period."
- False-positive controls: ignore rows with approved SARS arrangement references and documented deferral authority.
- Why this matters: timeliness failures increase penalties and compliance risk.

### Rule VAT_REG_001 - VAT Return Reconciliation Inconsistency

- Rule class: `Regulatory`.
- Detects: mismatch between VAT201 declared totals and ledger-derived VAT totals.
- Trigger formula: `abs(input_vat - vat_input_from_ledger) / max(input_vat,1) > 0.05 OR abs(output_vat - vat_output_from_ledger) / max(output_vat,1) > 0.05`.
- Required inputs: `input_vat`, `output_vat`, VAT-tagged ledger totals, `vat201_reference`.
- Weight: `+20`.
- Severity: `Warning`.
- Alert message: "VAT201 values do not reconcile with VAT-tagged ledger totals."
- False-positive controls: skip if credit-note adjustment is documented and linked.
- Why this matters: reconciliation gaps can lead to SARS queries.

### Rule VAT_REG_002 - Missing VAT Documentary Evidence

- Rule class: `Regulatory`.
- Detects: VAT input claims lacking invoice evidence fields.
- Trigger formula: any VATInput row where `tax_invoice_number` or `supplier_vat_number` or `tax_invoice_date` is null.
- Required inputs: transaction evidence columns.
- Weight: `+10`.
- Severity: `Warning`.
- Alert message: "VAT input claim has missing documentary evidence fields."
- False-positive controls: allow temporary hold state before filing deadline.
- Why this matters: insufficient evidence weakens VAT input claim defensibility.

## Heuristic Rules

### Rule VAT_HEU_001 - High VAT Refund Ratio

- Rule class: `Heuristic`.
- Detects: unusually high VAT refund claims relative to turnover.
- Trigger formula: `vat_refund_claimed / declared_turnover > 0.30`.
- Required inputs: `vat_refund_claimed`, `declared_turnover`.
- Weight: `+12`.
- Severity: `Warning`.
- Alert message: "VAT refund is unusually high relative to declared turnover."
- False-positive controls: skip when `declared_turnover < 10000` ZAR.
- Why this matters: refund outliers can increase verification attention.
- Note: internal heuristic threshold, not a statutory SARS limit.

### Rule EXP_HEU_001 - High Entertainment Expense Ratio

- Rule class: `Heuristic`.
- Detects: entertainment spending above expected threshold.
- Trigger formula: `entertainment_expense / total_expenses > 0.08`.
- Required inputs: `entertainment_expense`, `total_expenses`.
- Weight: `+8`.
- Severity: `Warning`.
- Alert message: "Entertainment expense ratio is above expected threshold."
- False-positive controls: skip if industry is `Hospitality`.
- Why this matters: non-core expense spikes may indicate misclassification risk.
- Note: internal heuristic threshold, not a statutory SARS limit.

### Rule REV_HEU_001 - Revenue Volatility Spike

- Rule class: `Heuristic`.
- Detects: abrupt month-over-month revenue swings.
- Trigger formula: `abs(current_revenue - previous_revenue) / max(previous_revenue,1) > 0.40`.
- Required inputs: monthly revenue series.
- Weight: `+5`.
- Severity: `Warning`.
- Alert message: "Revenue declaration volatility is unusually high month over month."
- False-positive controls: require at least 3 months of history.
- Why this matters: erratic declarations may trigger additional review.
- Note: internal heuristic threshold, not a statutory SARS limit.

### Rule EXP_HEU_002 - Round-Number Expense Clustering

- Rule class: `Heuristic`.
- Detects: suspicious concentration of rounded expense values.
- Trigger formula: `round_number_expense_count / total_expense_count > 0.35`.
- Required inputs: expense transaction amounts.
- Weight: `+3`.
- Severity: `Info`.
- Alert message: "Expense transactions show unusual round-number clustering."
- False-positive controls: exclude regulated fixed-fee categories.
- Why this matters: synthetic patterns can indicate poor controls or manipulation.
- Note: internal heuristic threshold, not a statutory SARS limit.

### Rule REP_HEU_001 - Repeated Submission Anomalies

- Rule class: `Heuristic`.
- Detects: recurring anomalies across recent filing periods.
- Trigger formula: anomaly flags present in `>= 3` consecutive periods.
- Required inputs: historical period rule outcomes.
- Weight: `+2`.
- Severity: `Warning`.
- Alert message: "Repeated anomalies detected across recent submission periods."
- False-positive controls: reset streak after verified corrective action.
- Why this matters: persistent issues increase compliance escalation risk.
- Note: internal heuristic threshold, not a statutory SARS limit.

## Rule Output Contract

Each triggered rule returns:

- `ruleCode`
- `ruleName`
- `ruleClass` (`Regulatory` or `Heuristic`)
- `severity` (`Info`, `Warning`, `Critical`)
- `weight`
- `description`
- `recommendation`
- `evidence` (key metrics used in trigger)
