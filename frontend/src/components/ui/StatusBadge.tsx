import type { AlertSeverity, RiskLevel } from '@/lib/api/types';

type BadgeTone = 'neutral' | 'success' | 'warning' | 'danger';

function toneForRiskLevel(level: RiskLevel): BadgeTone {
  switch (level) {
    case 'Low':
      return 'success';
    case 'Medium':
      return 'warning';
    case 'High':
      return 'danger';
  }
}

function toneForSeverity(severity: AlertSeverity): BadgeTone {
  switch (severity) {
    case 'Info':
      return 'neutral';
    case 'Warning':
      return 'warning';
    case 'Critical':
      return 'danger';
  }
}

interface StatusBadgeProps {
  riskLevel?: RiskLevel;
  severity?: AlertSeverity;
  children?: string;
}

export function StatusBadge({ riskLevel, severity, children }: StatusBadgeProps) {
  const tone = riskLevel ? toneForRiskLevel(riskLevel) : severity ? toneForSeverity(severity) : 'neutral';
  const label = children ?? riskLevel ?? severity ?? 'Status';

  return <span className={`badge badge-${tone}`}>{label}</span>;
}
