# TaxTrack Scoring Model v1

## Score Formula

Two deterministic components are calculated:

1. `regulatory_score` in range `0-70`
2. `heuristic_score` in range `0-30`

Final score:

`risk_score = min(100, regulatory_score + heuristic_score)`

No stochastic behavior is allowed in MVP.

## Risk Level Mapping

| Score Range | Risk Level | Interpretation |
|---|---|---|
| 0-40 | Low | baseline compliance posture appears stable |
| 41-70 | Medium | caution zone, review and corrective action recommended |
| 71-100 | High | elevated audit risk indicators, immediate review required |

Boundary behavior:

- `40` => Low
- `41` => Medium
- `70` => Medium
- `71` => High

## Rule Weights (v1)

| Rule Code | Rule Class | Weight |
|---|---|---|
| PAYE_REG_001 | Regulatory | 30 |
| PAYE_REG_002 | Regulatory | 25 |
| PAYE_REG_003 | Regulatory | 15 |
| VAT_REG_001 | Regulatory | 20 |
| VAT_REG_002 | Regulatory | 10 |
| VAT_HEU_001 | Heuristic | 12 |
| EXP_HEU_001 | Heuristic | 8 |
| REV_HEU_001 | Heuristic | 5 |
| EXP_HEU_002 | Heuristic | 3 |
| REP_HEU_001 | Heuristic | 2 |

Scoring caps:

- `regulatory_score = min(70, sum(regulatory_weights_triggered_scaled))`
- `heuristic_score = min(30, sum(heuristic_weights_triggered_scaled))`

Regulatory and heuristic weight sets are normalized to component caps to prevent one class from dominating due to raw count.

## False Alarm Controls

1. Minimum scoring-confidence threshold: 80 percent statutory evidence completeness. Lower values set `insufficientEvidence=true` and require manual review.
2. Rules with historical dependency require minimum history availability.
3. Industry-specific exceptions are documented in rule definitions.
4. Trigger evidence must be attached to each alert for explainability.
5. Missing statutory evidence lowers confidence and sets `insufficientEvidence=true`.

## Scoring Output Contract

The analysis response must include:

- `riskScore` (integer 0-100)
- `regulatoryScore` (integer 0-70)
- `heuristicScore` (integer 0-30)
- `riskLevel` (`Low`, `Medium`, `High`)
- `taxPolicyVersion` (string)
- `policyEffectiveDate` (date)
- `evidenceCompleteness` (integer 0-100)
- `insufficientEvidence` (boolean)
- `triggeredRules` (array of rule results)
- `alerts` (array mapped from triggered rules)
- `generatedAt` (UTC timestamp)

## Determinism Requirements

1. Same input data and same contract version must produce the same score.
2. Rule execution order must not change score output.
3. Floating-point operations must be rounded to 2 decimal places before threshold comparison where applicable.
4. Same input data with a different policy-effective-date may produce different outcomes only when policy values differ by date.

## Policy Data Requirements

Policy configuration must be externalized and versioned, including at least:

- `vatStandardRate`
- `vatCompulsoryRegistrationThresholdZar`
- `vatVoluntaryRegistrationThresholdZar`
- `emp201DueDayOfMonth`
- `publicHolidayDates`
