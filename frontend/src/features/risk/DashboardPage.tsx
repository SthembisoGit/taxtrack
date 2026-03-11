import { useEffect, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTable } from '@/components/ui/AlertTable';
import { Panel } from '@/components/ui/Panel';
import { RiskSummaryCard } from '@/components/ui/RiskSummaryCard';
import { apiClient, ApiError } from '@/lib/api/client';
import { useAuthSession } from '@/lib/auth/session';
import { generateIdempotencyKey } from '@/lib/utils/idempotency';

export function DashboardPage() {
  const session = useAuthSession();
  const companyId = session?.selectedCompany?.id;

  if (!companyId) {
    return (
      <Panel title="No company selected" subtitle="Create a company profile before running analysis.">
        <p className="empty-copy">Analysis is always scoped to a single company in this MVP slice.</p>
      </Panel>
    );
  }

  return (
    <DashboardWorkspace
      key={companyId}
      companyId={companyId}
      companyName={session.selectedCompany?.name ?? ''}
      registrationNumber={session.selectedCompany?.registrationNumber ?? ''}
    />
  );
}

interface DashboardWorkspaceProps {
  companyId: string;
  companyName: string;
  registrationNumber: string;
}

function DashboardWorkspace({ companyId, companyName, registrationNumber }: DashboardWorkspaceProps) {
  const queryClient = useQueryClient();
  const [analysisId, setAnalysisId] = useState('');
  const [error, setError] = useState('');

  const riskQuery = useQuery({
    queryKey: ['risk-result', companyId],
    queryFn: async () => {
      return apiClient.getLatestRisk(companyId);
    },
    retry: false,
  });

  const analyzeMutation = useMutation({
    mutationFn: async () => {
      if (!companyId) {
        throw new Error('No company selected.');
      }

      return apiClient.analyzeRisk({
        companyId,
        idempotencyKey: generateIdempotencyKey(),
      });
    },
    onSuccess: (response) => {
      setError('');
      setAnalysisId(response.analysisId);
    },
    onError: (caught) => {
      if (caught instanceof ApiError) {
        setError(caught.problem.detail);
        return;
      }

      setError('We could not start analysis.');
    },
  });

  const analysisStatusQuery = useQuery({
    queryKey: ['analysis-status', analysisId],
    enabled: Boolean(analysisId),
    queryFn: () => apiClient.getAnalysisStatus(analysisId),
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      if (!status || status === 'Completed' || status === 'Failed') {
        return false;
      }

      return 2000;
    },
  });

  useEffect(() => {
    if (analysisStatusQuery.data?.status === 'Completed' && companyId) {
      void queryClient.invalidateQueries({ queryKey: ['risk-result', companyId] });
      void queryClient.invalidateQueries({ queryKey: ['report', companyId] });
    }
  }, [analysisStatusQuery.data?.status, companyId, queryClient]);

  const notFound = riskQuery.error instanceof ApiError && riskQuery.error.problem.status === 404;

  return (
    <div className="stack gap-lg">
      <div className="page-heading page-heading-row">
        <div>
          <p className="eyebrow">Risk dashboard</p>
          <h1>Review the latest tax posture for {companyName}.</h1>
        </div>
        <button
          className="button button-primary"
          disabled={analyzeMutation.isPending}
          onClick={() => analyzeMutation.mutate()}
          type="button"
        >
          {analyzeMutation.isPending ? 'Running analysis...' : 'Run analysis'}
        </button>
      </div>

      {error ? <div className="banner banner-error">{error}</div> : null}

      {analysisStatusQuery.data ? (
        <Panel title="Analysis job" subtitle={`Job ID ${analysisStatusQuery.data.analysisId}`}>
          <div className="summary-grid">
            <div className="summary-metric">
              <span className="metric-label">Status</span>
              <strong>{analysisStatusQuery.data.status}</strong>
            </div>
            <div className="summary-metric">
              <span className="metric-label">Updated</span>
              <strong>{new Date(analysisStatusQuery.data.updatedAtUtc).toLocaleString()}</strong>
            </div>
          </div>
        </Panel>
      ) : null}

      {riskQuery.isLoading ? (
        <Panel title="Loading dashboard" subtitle="Pulling the latest risk result.">
          <div className="skeleton-grid">
            <div className="skeleton-block" />
            <div className="skeleton-block" />
            <div className="skeleton-block" />
          </div>
        </Panel>
      ) : null}

      {notFound ? (
        <Panel title="No analysis yet" subtitle="Run the first analysis after uploading at least one dataset.">
          <p className="empty-copy">TaxTrack will show score, evidence quality, and triggered alerts here.</p>
        </Panel>
      ) : null}

      {riskQuery.data ? (
        <>
          <RiskSummaryCard summary={riskQuery.data} />

          <div className="summary-grid">
            <Panel title="Trigger count">
              <p className="metric-callout">{riskQuery.data.alerts.length}</p>
              <p className="metric-caption">Active rule-based warnings</p>
            </Panel>
            <Panel title="Generated at">
              <p className="metric-callout">{new Date(riskQuery.data.generatedAtUtc).toLocaleString()}</p>
              <p className="metric-caption">Latest completed analysis</p>
            </Panel>
            <Panel title="Company">
              <p className="metric-callout">{registrationNumber}</p>
              <p className="metric-caption">{companyName}</p>
            </Panel>
          </div>

          <Panel
            title="Alerts"
            subtitle="Every alert is tied to a documented rule code and remediation note."
          >
            <AlertTable alerts={riskQuery.data.alerts} />
          </Panel>
        </>
      ) : null}
    </div>
  );
}
