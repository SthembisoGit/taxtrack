import { useQuery } from '@tanstack/react-query';
import { AlertTable } from '@/components/ui/AlertTable';
import { Panel } from '@/components/ui/Panel';
import { RiskSummaryCard } from '@/components/ui/RiskSummaryCard';
import { apiClient, ApiError } from '@/lib/api/client';
import { useAuthSession } from '@/lib/auth/session';

export function ReportPage() {
  const session = useAuthSession();
  const companyId = session?.selectedCompany?.id;

  const reportQuery = useQuery({
    queryKey: ['report', companyId],
    enabled: Boolean(companyId),
    queryFn: async () => {
      if (!companyId) {
        return null;
      }

      return apiClient.getReport(companyId);
    },
    retry: false,
  });

  if (!companyId) {
    return (
      <Panel title="No company selected" subtitle="Set the active company before requesting reports.">
        <p className="empty-copy">Reports are generated against the selected company context.</p>
      </Panel>
    );
  }

  const notFound = reportQuery.error instanceof ApiError && reportQuery.error.problem.status === 404;

  return (
    <div className="stack gap-lg">
      <div className="page-heading">
        <p className="eyebrow">Compliance report</p>
        <h1>Download-ready report data for {session.selectedCompany?.name}.</h1>
      </div>

      {reportQuery.isLoading ? (
        <Panel title="Generating report" subtitle="Preparing the latest report payload.">
          <div className="skeleton-grid">
            <div className="skeleton-block" />
            <div className="skeleton-block" />
          </div>
        </Panel>
      ) : null}

      {notFound ? (
        <Panel title="No report yet" subtitle="Run analysis before opening report downloads.">
          <p className="empty-copy">The report view mirrors the latest completed dashboard analysis.</p>
        </Panel>
      ) : null}

      {reportQuery.data ? (
        <>
          <RiskSummaryCard summary={reportQuery.data.riskSummary} title="Report summary" />

          <Panel
            title="Download options"
            subtitle={`Generated ${new Date(reportQuery.data.generatedAt).toLocaleString()}`}
          >
            <div className="download-list">
              {reportQuery.data.downloadOptions.map((download) => (
                <a
                  key={download.format}
                  className="download-card"
                  href={download.url}
                  rel="noreferrer"
                  target="_blank"
                >
                  <strong>{download.format.toUpperCase()}</strong>
                  <span>Expires {new Date(download.expiresAt).toLocaleString()}</span>
                </a>
              ))}
            </div>
          </Panel>

          <Panel
            title="Triggered alerts"
            subtitle="This list intentionally mirrors the dashboard output for consistency."
          >
            <AlertTable alerts={reportQuery.data.alerts} />
          </Panel>
        </>
      ) : null}
    </div>
  );
}
