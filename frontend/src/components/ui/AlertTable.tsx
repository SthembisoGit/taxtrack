import { StatusBadge } from '@/components/ui/StatusBadge';
import type { RiskAlert } from '@/lib/api/types';

interface AlertTableProps {
  alerts: RiskAlert[];
}

export function AlertTable({ alerts }: AlertTableProps) {
  if (!alerts.length) {
    return <p className="empty-copy">No active alerts. Trigger an analysis to populate this view.</p>;
  }

  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead>
          <tr>
            <th>Rule</th>
            <th>Severity</th>
            <th>Description</th>
            <th>Recommendation</th>
          </tr>
        </thead>
        <tbody>
          {alerts.map((alert) => (
            <tr key={alert.ruleCode}>
              <td>{alert.ruleCode}</td>
              <td>
                <StatusBadge severity={alert.severity} />
              </td>
              <td>{alert.description}</td>
              <td>{alert.recommendation}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
