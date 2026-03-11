import { useMemo, useState, type FormEvent } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { InputField, SelectField, TextAreaField } from '@/components/ui/FormField';
import { Panel } from '@/components/ui/Panel';
import { apiClient, ApiError } from '@/lib/api/client';
import { useAuthSession } from '@/lib/auth/session';
import type {
  DataSubjectRequestResponse,
  DataSubjectRequestStatus,
  DataSubjectRequestType,
} from '@/lib/api/types';

const requestTypes: DataSubjectRequestType[] = ['Export', 'Deletion'];

function getStatusBadgeClass(status: DataSubjectRequestStatus) {
  switch (status) {
    case 'Completed':
      return 'badge badge-success';
    case 'Rejected':
      return 'badge badge-danger';
    case 'InProgress':
      return 'badge badge-warning';
    default:
      return 'badge badge-neutral';
  }
}

function PrivacyRequestSummary({ request }: { request: DataSubjectRequestResponse }) {
  return (
    <div className="summary-grid">
      <div className="summary-metric">
        <span className="metric-label">Request ID</span>
        <strong className="privacy-code">{request.requestId}</strong>
      </div>
      <div className="summary-metric">
        <span className="metric-label">Type</span>
        <strong>{request.requestType}</strong>
      </div>
      <div className="summary-metric">
        <span className="metric-label">Status</span>
        <strong>
          <span className={getStatusBadgeClass(request.status)}>{request.status}</span>
        </strong>
      </div>
      <div className="summary-metric">
        <span className="metric-label">Updated</span>
        <strong>{new Date(request.updatedAtUtc).toLocaleString()}</strong>
      </div>
      {request.resolutionNote ? (
        <div className="summary-metric full-span">
          <span className="metric-label">Resolution note</span>
          <strong>{request.resolutionNote}</strong>
        </div>
      ) : null}
    </div>
  );
}

export function PrivacyPage() {
  const session = useAuthSession();
  const [scope, setScope] = useState<'personal' | 'workspace'>(
    session?.selectedCompany ? 'workspace' : 'personal',
  );
  const [requestType, setRequestType] = useState<DataSubjectRequestType>('Export');
  const [reason, setReason] = useState('');
  const [lookupRequestId, setLookupRequestId] = useState('');
  const [activeRequestId, setActiveRequestId] = useState('');
  const [submitError, setSubmitError] = useState('');
  const [lookupError, setLookupError] = useState('');

  const selectedCompany = session?.selectedCompany;
  const canUseWorkspaceScope = Boolean(selectedCompany);
  const resolvedScope = canUseWorkspaceScope ? scope : 'personal';

  const activeRequestQuery = useQuery({
    queryKey: ['privacy-request', activeRequestId],
    enabled: Boolean(activeRequestId),
    queryFn: () => apiClient.getDataRequest(activeRequestId),
    retry: false,
  });

  const createMutation = useMutation({
    mutationFn: async () => {
      return apiClient.createDataRequest({
        companyId: resolvedScope === 'workspace' ? selectedCompany?.id : undefined,
        requestType,
        reason: reason.trim() || undefined,
      });
    },
    onSuccess: (response) => {
      setSubmitError('');
      setLookupError('');
      setActiveRequestId(response.requestId);
      setLookupRequestId(response.requestId);
      setReason('');
    },
    onError: (caught) => {
      if (caught instanceof ApiError) {
        setSubmitError(caught.problem.detail);
        return;
      }

      setSubmitError('We could not submit the privacy request.');
    },
  });

  const lookupMutation = useMutation({
    mutationFn: async () => {
      return apiClient.getDataRequest(lookupRequestId.trim());
    },
    onSuccess: (response) => {
      setLookupError('');
      setActiveRequestId(response.requestId);
    },
    onError: (caught) => {
      if (caught instanceof ApiError) {
        setLookupError(caught.problem.detail);
        return;
      }

      setLookupError('We could not load that privacy request.');
    },
  });

  const activeRequest = useMemo(() => activeRequestQuery.data ?? null, [activeRequestQuery.data]);
  const activeRequestError =
    activeRequestQuery.error instanceof ApiError ? activeRequestQuery.error.problem.detail : '';

  function handleLookup(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLookupError('');

    if (!lookupRequestId.trim()) {
      setLookupError('Enter a request ID to look up.');
      return;
    }

    lookupMutation.mutate();
  }

  return (
    <div className="stack gap-lg">
      <div className="page-heading">
        <p className="eyebrow">Privacy requests</p>
        <h1>Record POPIA export and deletion requests without leaving the workspace.</h1>
      </div>

      <Panel
        title="Create a privacy request"
        subtitle="Record a data export or deletion request for yourself or the active workspace."
      >
        <form
          className="grid-form"
          noValidate
          onSubmit={(event) => {
            event.preventDefault();
            createMutation.mutate();
          }}
        >
          {submitError ? <div className="banner banner-error full-span">{submitError}</div> : null}

          <SelectField
            label="Scope"
            onChange={(event) => setScope(event.target.value as 'personal' | 'workspace')}
            value={resolvedScope}
          >
            <option value="personal">My account</option>
            <option disabled={!canUseWorkspaceScope} value="workspace">
              {canUseWorkspaceScope
                ? `Selected workspace: ${selectedCompany?.name}`
                : 'Selected workspace unavailable'}
            </option>
          </SelectField>

          <SelectField
            label="Request type"
            onChange={(event) => setRequestType(event.target.value as DataSubjectRequestType)}
            value={requestType}
          >
            {requestTypes.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </SelectField>

          <TextAreaField
            helperText="Add concise context for auditability. This note is optional."
            label="Reason"
            onChange={(event) => setReason(event.target.value)}
            placeholder="Example: Customer requested a copy of submitted financial data."
            value={reason}
          />

          <div className="privacy-note full-span">
            {resolvedScope === 'workspace' ? (
              <p className="empty-copy">
                This request will be linked to <strong>{selectedCompany?.name}</strong> and recorded
                in the audit trail.
              </p>
            ) : (
              <p className="empty-copy">
                This request will be recorded as a personal account request without a workspace
                association.
              </p>
            )}
          </div>

          <div className="button-row full-span">
            <button className="button button-primary" disabled={createMutation.isPending} type="submit">
              {createMutation.isPending ? 'Submitting...' : 'Submit privacy request'}
            </button>
          </div>
        </form>
      </Panel>

      <Panel
        title="Track an existing request"
        subtitle="Use a known request ID to check the latest recorded status."
      >
        <form className="grid-form" noValidate onSubmit={handleLookup}>
          {lookupError ? <div className="banner banner-error full-span">{lookupError}</div> : null}

          <InputField
            label="Request ID"
            onChange={(event) => setLookupRequestId(event.target.value)}
            placeholder="Paste a request UUID"
            value={lookupRequestId}
          />

          <div className="button-row align-end">
            <button className="button button-secondary" disabled={lookupMutation.isPending} type="submit">
              {lookupMutation.isPending ? 'Checking...' : 'Load request'}
            </button>
          </div>
        </form>
      </Panel>

      {activeRequestQuery.isLoading ? (
        <Panel title="Loading privacy request" subtitle="Retrieving the latest recorded status.">
          <div className="skeleton-grid">
            <div className="skeleton-block" />
            <div className="skeleton-block" />
          </div>
        </Panel>
      ) : null}

      {activeRequestError ? (
        <Panel title="Privacy request retrieval failed" subtitle="We could not load the selected request.">
          <div className="banner banner-error">{activeRequestError}</div>
        </Panel>
      ) : null}

      {activeRequest ? (
        <Panel
          title="Request status"
          subtitle={`Created ${new Date(activeRequest.createdAtUtc).toLocaleString()}`}
        >
          <PrivacyRequestSummary request={activeRequest} />
        </Panel>
      ) : null}
    </div>
  );
}
