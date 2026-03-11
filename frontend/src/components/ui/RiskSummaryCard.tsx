import { Panel } from '@/components/ui/Panel';
import { StatusBadge } from '@/components/ui/StatusBadge';
import type { RiskResultResponse } from '@/lib/api/types';

interface RiskSummaryCardProps {
  summary: RiskResultResponse;
  title?: string;
}

export function RiskSummaryCard({ summary, title = 'Tax Risk Score' }: RiskSummaryCardProps) {
  return (
    <Panel
      title={title}
      subtitle={`Policy ${summary.taxPolicyVersion} | Effective ${summary.policyEffectiveDate}`}
      actions={<StatusBadge riskLevel={summary.riskLevel} />}
      className="risk-summary-card"
    >
      <div className="score-grid">
        <div>
          <p className="score-value">{summary.riskScore}</p>
          <p className="score-caption">Overall score</p>
        </div>
        <div className="score-breakdown">
          <div>
            <span className="metric-label">Regulatory</span>
            <strong>{summary.regulatoryScore}</strong>
          </div>
          <div>
            <span className="metric-label">Heuristic</span>
            <strong>{summary.heuristicScore}</strong>
          </div>
          <div>
            <span className="metric-label">Evidence</span>
            <strong>{summary.evidenceCompleteness}%</strong>
          </div>
        </div>
      </div>
      {summary.insufficientEvidence ? (
        <p className="warning-inline">
          Evidence is incomplete. Review source documents before relying on this score.
        </p>
      ) : null}
    </Panel>
  );
}
