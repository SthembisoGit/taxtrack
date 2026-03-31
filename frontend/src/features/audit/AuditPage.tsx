import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Panel } from '@/components/ui/Panel';
import { QueryErrorPanel } from '@/components/ui/QueryErrorPanel';
import { apiClient, ApiError } from '@/lib/api/client';
import { useAuthSession } from '@/lib/auth/session';
import type { AuditEventType, AuditLogEventResponse } from '@/lib/api/types';

const companyAuditQueryKey = (userId: string | undefined, companyId: string) =>
  ['audit-log', 'company', userId ?? 'anonymous', companyId] as const;
const securityAuditQueryKey = (userId?: string) => ['audit-log', 'security', userId ?? 'anonymous'] as const;

const auditEventLabels: Record<AuditEventType, string> = {
  LoginSucceeded: 'Login succeeded',
  LoginFailed: 'Login failed',
  UploadCreated: 'Upload created',
  UploadValidationFailed: 'Upload validation failed',
  RiskAnalysisRequested: 'Risk analysis requested',
  RiskAnalysisCompleted: 'Risk analysis completed',
  ReportDownloaded: 'Report downloaded',
  DataExportRequested: 'Data export requested',
  DataDeletionRequested: 'Data deletion requested',
  DataBreachNotified: 'Breach notified',
  TokenRefreshed: 'Token refreshed',
};

function summarizeMetadata(metadataJson: string) {
  try {
    const parsed = JSON.parse(metadataJson) as Record<string, unknown>;
    const entries = Object.entries(parsed).slice(0, 3);
    if (!entries.length) {
      return 'No metadata';
    }

    return entries
      .map(([key, value]) => `${key}: ${String(value)}`)
      .join(' | ');
  } catch {
    return metadataJson;
  }
}

function AuditTable({
  events,
  emptyMessage,
}: {
  events: AuditLogEventResponse[];
  emptyMessage: string;
}) {
  if (!events.length) {
    return <p className="empty-copy">{emptyMessage}</p>;
  }

  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead>
          <tr>
            <th>Event</th>
            <th>Actor</th>
            <th>Timestamp</th>
            <th>Correlation</th>
            <th>Metadata</th>
          </tr>
        </thead>
        <tbody>
          {events.map((event) => (
            <tr key={event.eventId}>
              <td>
                <div className="audit-cell">
                  <strong>{auditEventLabels[event.eventType]}</strong>
                  <span className="audit-code">{event.eventType}</span>
                </div>
              </td>
              <td>{event.actorEmail}</td>
              <td>{new Date(event.eventTimeUtc).toLocaleString()}</td>
              <td>
                <code className="audit-code">{event.correlationId}</code>
              </td>
              <td>
                <details className="audit-details">
                  <summary>{summarizeMetadata(event.metadataJson)}</summary>
                  <pre className="audit-pre">{event.metadataJson}</pre>
                </details>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function AuditPage() {
  const session = useAuthSession();
  const userId = session?.userId;
  const companyId = session?.selectedCompany?.id;

  const companyQuery = useQuery({
    queryKey: companyId
      ? companyAuditQueryKey(userId, companyId)
      : ['audit-log', 'company', userId ?? 'anonymous', 'none'],
    enabled: Boolean(companyId),
    queryFn: () => apiClient.getAuditLog({ companyId: companyId ?? undefined, limit: 25 }),
    retry: false,
  });

  const securityQuery = useQuery({
    queryKey: securityAuditQueryKey(userId),
    queryFn: () => apiClient.getAuditLog({ limit: 15 }),
    retry: false,
  });

  const companyEvents = useMemo(() => companyQuery.data ?? [], [companyQuery.data]);
  const securityEvents = useMemo(() => securityQuery.data ?? [], [securityQuery.data]);
  const companyError = companyQuery.error instanceof ApiError ? companyQuery.error.problem.detail : '';
  const securityError = securityQuery.error instanceof ApiError ? securityQuery.error.problem.detail : '';

  return (
    <div className="stack gap-lg">
      <div className="page-heading">
        <p className="eyebrow">Audit trail</p>
        <h1>Review sensitive activity for compliance and investigation.</h1>
      </div>

      <Panel
        title="Workspace activity"
        subtitle={
          companyId
            ? `Latest activity for ${session?.selectedCompany?.name}`
            : 'Select a workspace to load company-scoped activity.'
        }
      >
        {!companyId ? (
          <p className="empty-copy">Workspace audit events appear once a company is selected.</p>
        ) : companyQuery.isLoading ? (
          <div className="skeleton-grid">
            <div className="skeleton-block" />
            <div className="skeleton-block" />
          </div>
        ) : companyError ? (
          <QueryErrorPanel
            embedded
            message={companyError}
            onRetry={() => {
              void companyQuery.refetch();
            }}
            retrying={companyQuery.isFetching}
            subtitle="We could not load workspace-scoped audit activity."
            title="Workspace audit retrieval failed"
          />
        ) : (
          <AuditTable
            emptyMessage="No workspace audit events have been recorded yet."
            events={companyEvents}
          />
        )}
      </Panel>

      <Panel
        title="My security events"
        subtitle="Global events linked to your own account, including login and token activity."
      >
        {securityQuery.isLoading ? (
          <div className="skeleton-grid">
            <div className="skeleton-block" />
            <div className="skeleton-block" />
          </div>
        ) : securityError ? (
          <QueryErrorPanel
            embedded
            message={securityError}
            onRetry={() => {
              void securityQuery.refetch();
            }}
            retrying={securityQuery.isFetching}
            subtitle="We could not load account-level security activity."
            title="Security audit retrieval failed"
          />
        ) : (
          <AuditTable
            emptyMessage="No personal security events have been recorded yet."
            events={securityEvents}
          />
        )}
      </Panel>
    </div>
  );
}
