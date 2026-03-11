import type { ReportResponse, RiskAlert, RiskResultResponse } from '@/lib/api/types';

function normalizeAlertSignature(alert: RiskAlert) {
  return `${alert.ruleCode}:${alert.severity}:${alert.type}`;
}

function getSortedAlertSignatures(alerts: RiskAlert[]) {
  return alerts.map(normalizeAlertSignature).sort();
}

function sameAlertSet(left: RiskAlert[], right: RiskAlert[]) {
  const leftSignatures = getSortedAlertSignatures(left);
  const rightSignatures = getSortedAlertSignatures(right);

  if (leftSignatures.length !== rightSignatures.length) {
    return false;
  }

  return leftSignatures.every((signature, index) => signature === rightSignatures[index]);
}

export function getReportConsistencyIssues(
  report: ReportResponse,
  latestRisk: RiskResultResponse | null,
) {
  const issues: string[] = [];

  if (!sameAlertSet(report.alerts, report.riskSummary.alerts)) {
    issues.push('Report alert details do not match the embedded risk summary snapshot.');
  }

  if (!latestRisk) {
    issues.push('The latest dashboard result is unavailable for comparison.');
    return issues;
  }

  if (report.riskSummary.resultId !== latestRisk.resultId) {
    issues.push('Report snapshot is not using the latest completed analysis result.');
  }

  if (
    report.riskSummary.riskScore !== latestRisk.riskScore ||
    report.riskSummary.riskLevel !== latestRisk.riskLevel
  ) {
    issues.push('Report score or risk level differs from the latest dashboard result.');
  }

  if (
    report.riskSummary.taxPolicyVersion !== latestRisk.taxPolicyVersion ||
    report.riskSummary.policyEffectiveDate !== latestRisk.policyEffectiveDate
  ) {
    issues.push('Report policy metadata differs from the latest dashboard result.');
  }

  if (!sameAlertSet(report.alerts, latestRisk.alerts)) {
    issues.push('Report alerts differ from the latest dashboard result.');
  }

  return issues;
}
