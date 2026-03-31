import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { AlertTable } from '@/components/ui/AlertTable';
import { Panel } from '@/components/ui/Panel';
import { QueryErrorPanel } from '@/components/ui/QueryErrorPanel';
import { RiskSummaryCard } from '@/components/ui/RiskSummaryCard';
import { apiClient, ApiError } from '@/lib/api/client';
import { useAuthSession } from '@/lib/auth/session';
import { getReportConsistencyIssues } from '@/features/report/consistency';

export function ReportPage() {
  const session = useAuthSession();
  const userId = session?.userId;
  const companyId = session?.selectedCompany?.id;
  const [downloadError, setDownloadError] = useState('');
  const [downloadingFormat, setDownloadingFormat] = useState('');

  const reportQuery = useQuery({
    queryKey: ['report', userId, companyId],
    enabled: Boolean(companyId),
    queryFn: async () => {
      if (!companyId) {
        return null;
      }

      return apiClient.getReport(companyId);
    },
    retry: false,
  });

  const latestRiskQuery = useQuery({
    queryKey: ['risk-result', userId, companyId],
    enabled: Boolean(companyId),
    queryFn: async () => {
      if (!companyId) {
        return null;
      }

      return apiClient.getLatestRisk(companyId);
    },
    retry: false,
  });

  const reportError = reportQuery.error instanceof ApiError ? reportQuery.error : null;
  const latestRiskApiError = latestRiskQuery.error instanceof ApiError ? latestRiskQuery.error : null;
  const notFound = reportError?.problem.status === 404;
  const loadError = reportError ? reportError.problem.status !== 404 : false;
  const reportErrorDetail = reportError && loadError ? reportError.problem.detail : '';
  const latestRiskMissing = latestRiskApiError?.problem.status === 404;
  const latestRiskError =
    latestRiskApiError && latestRiskApiError.problem.status !== 404
      ? latestRiskApiError.problem.detail
      : '';

  async function handleDownload(downloadUrl: string, format: string) {
    setDownloadingFormat(format);
    setDownloadError('');

    try {
      const { blob, fileName } = await apiClient.downloadReport(downloadUrl);
      const objectUrl = window.URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = objectUrl;
      anchor.download = fileName;
      document.body.appendChild(anchor);
      anchor.click();
      anchor.remove();
      window.URL.revokeObjectURL(objectUrl);
    } catch (caught) {
      if (caught instanceof ApiError) {
        setDownloadError(caught.problem.detail);
      } else if (caught instanceof Error) {
        setDownloadError(caught.message);
      } else {
        setDownloadError('We could not download the report artifact.');
      }
    } finally {
      setDownloadingFormat('');
    }
  }

  const consistencyIssues = useMemo(() => {
    if (!reportQuery.data) {
      return [];
    }

    if (latestRiskError) {
      return [`Consistency check could not complete: ${latestRiskError}`];
    }

    return getReportConsistencyIssues(reportQuery.data, latestRiskMissing ? null : latestRiskQuery.data ?? null);
  }, [latestRiskError, latestRiskMissing, latestRiskQuery.data, reportQuery.data]);

  if (!companyId) {
    return (
      <Panel title="No company selected" subtitle="Set the active company before requesting reports.">
        <p className="empty-copy">Reports are generated against the selected company context.</p>
      </Panel>
    );
  }

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

      {loadError ? (
        <QueryErrorPanel
          message={reportErrorDetail}
          onRetry={() => {
            void reportQuery.refetch();
          }}
          retrying={reportQuery.isFetching}
          subtitle="We could not load the latest report payload."
          title="Report retrieval failed"
        />
      ) : null}

      {notFound ? (
        <Panel title="No report yet" subtitle="Run analysis before opening report downloads.">
          <p className="empty-copy">The report view mirrors the latest completed dashboard analysis.</p>
        </Panel>
      ) : null}

      {reportQuery.data ? (
        <>
          {latestRiskQuery.isLoading ? (
            <Panel
              title="Checking report consistency"
              subtitle="Comparing this report snapshot with the latest dashboard result."
            >
              <div className="skeleton-grid">
                <div className="skeleton-block" />
              </div>
            </Panel>
          ) : null}

          {!latestRiskQuery.isLoading ? (
            <Panel
              title="Consistency check"
              subtitle="TaxTrack verifies that report output matches the latest dashboard analysis."
            >
              {latestRiskError ? (
                <div className="stack">
                  <div className="banner banner-error">
                    Consistency check could not complete: {latestRiskError}
                  </div>
                  <div className="button-row">
                    <button
                      className="button button-secondary"
                      disabled={latestRiskQuery.isFetching}
                      onClick={() => {
                        void latestRiskQuery.refetch();
                      }}
                      type="button"
                    >
                      {latestRiskQuery.isFetching ? 'Retrying...' : 'Retry consistency check'}
                    </button>
                  </div>
                </div>
              ) : consistencyIssues.length ? (
                <div className="banner banner-warning">
                  <p className="banner-title">Consistency issues detected.</p>
                  <ul className="banner-list">
                    {consistencyIssues.map((issue) => (
                      <li key={issue}>{issue}</li>
                    ))}
                  </ul>
                </div>
              ) : (
                <div className="banner banner-success">
                  Report summary and alert output match the latest dashboard analysis result.
                </div>
              )}
            </Panel>
          ) : null}

          <RiskSummaryCard summary={reportQuery.data.riskSummary} title="Report summary" />

        <Panel
          title="Download options"
          subtitle={`Generated ${new Date(reportQuery.data.generatedAtUtc).toLocaleString()}`}
        >
          {downloadError ? <div className="banner banner-error">{downloadError}</div> : null}
          {reportQuery.data.downloadOptions.length ? (
            <div className="download-list">
                {reportQuery.data.downloadOptions.map((download) => (
                  <button
                    key={download.format}
                    className="download-card"
                    onClick={() => {
                      void handleDownload(download.url, download.format);
                    }}
                    type="button"
                  >
                    <strong>{download.format.toUpperCase()}</strong>
                    <span>{downloadingFormat === download.format ? 'Downloading...' : 'Authenticated download'}</span>
                    <span>Expires {new Date(download.expiresAtUtc).toLocaleString()}</span>
                  </button>
                ))}
              </div>
            ) : (
              <p className="empty-copy">No download links are available for this report yet.</p>
            )}
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
