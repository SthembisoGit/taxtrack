import { useMemo, useState, type FormEvent } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
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
const privacyRequestsQueryKey = ['privacy-requests'] as const;

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

function upsertRequest(
  requests: DataSubjectRequestResponse[] | undefined,
  nextRequest: DataSubjectRequestResponse,
) {
  const existing = requests ?? [];
  return [nextRequest, ...existing.filter((request) => request.requestId !== nextRequest.requestId)];
}

function PrivacyRequestSummary({ request }: { request: DataSubjectRequestResponse }) {
  return (
    <div className="summary-grid">
      <div className="summary-metric">
        <span className="metric-label">Request ID</span>
        <strong className="privacy-code">{request.requestId}</strong>
      </div>
      <div className="summary-metric">
        <span className="metric-label">Scope</span>
        <strong>{request.companyId ? 'Workspace' : 'My account'}</strong>
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
        <span className="metric-label">Created</span>
        <strong>{new Date(request.createdAtUtc).toLocaleString()}</strong>
      </div>
      <div className="summary-metric">
        <span className="metric-label">Updated</span>
        <strong>{new Date(request.updatedAtUtc).toLocaleString()}</strong>
      </div>
      {request.reason ? (
        <div className="summary-metric full-span">
          <span className="metric-label">Reason</span>
          <strong>{request.reason}</strong>
        </div>
      ) : null}
      {request.resolutionNote ? (
        <div className="summary-metric full-span">
          <span className="metric-label">Resolution note</span>
          <strong>{request.resolutionNote}</strong>
        </div>
      ) : null}
    </div>
  );
}

function PrivacyRequestTable({
  requests,
  emptyMessage,
  onView,
}: {
  requests: DataSubjectRequestResponse[];
  emptyMessage: string;
  onView: (requestId: string) => void;
}) {
  if (!requests.length) {
    return <p className="empty-copy">{emptyMessage}</p>;
  }

  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead>
          <tr>
            <th>Request ID</th>
            <th>Type</th>
            <th>Status</th>
            <th>Updated</th>
            <th>Reason</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {requests.map((request) => (
            <tr key={request.requestId}>
              <td>
                <code className="privacy-code">{request.requestId}</code>
              </td>
              <td>{request.requestType}</td>
              <td>
                <span className={getStatusBadgeClass(request.status)}>{request.status}</span>
              </td>
              <td>{new Date(request.updatedAtUtc).toLocaleString()}</td>
              <td>{request.reason ?? 'No reason supplied'}</td>
              <td>
                <button
                  className="button button-secondary button-compact"
                  onClick={() => onView(request.requestId)}
                  type="button"
                >
                  View
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function PrivacyPage() {
  const queryClient = useQueryClient();
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

  const requestsQuery = useQuery({
    queryKey: privacyRequestsQueryKey,
    queryFn: () => apiClient.listDataRequests(),
    retry: false,
  });

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
      queryClient.setQueryData<DataSubjectRequestResponse[]>(
        privacyRequestsQueryKey,
        (current) => upsertRequest(current, response),
      );
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
      queryClient.setQueryData<DataSubjectRequestResponse[]>(
        privacyRequestsQueryKey,
        (current) => upsertRequest(current, response),
      );
    },
    onError: (caught) => {
      if (caught instanceof ApiError) {
        setLookupError(caught.problem.detail);
        return;
      }

      setLookupError('We could not load that privacy request.');
    },
  });

  const allRequests = useMemo(() => requestsQuery.data ?? [], [requestsQuery.data]);
  const workspaceRequests = useMemo(
    () => allRequests.filter((request) => request.companyId && request.companyId === selectedCompany?.id),
    [allRequests, selectedCompany?.id],
  );
  const personalRequests = useMemo(
    () => allRequests.filter((request) => !request.companyId),
    [allRequests],
  );
  const activeRequest = useMemo(() => activeRequestQuery.data ?? null, [activeRequestQuery.data]);
  const activeRequestError =
    activeRequestQuery.error instanceof ApiError ? activeRequestQuery.error.problem.detail : '';
  const listError = requestsQuery.error instanceof ApiError ? requestsQuery.error.problem.detail : '';

  function handleLookup(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLookupError('');

    if (!lookupRequestId.trim()) {
      setLookupError('Enter a request ID to look up.');
      return;
    }

    lookupMutation.mutate();
  }

  function handleViewRequest(requestId: string) {
    setLookupError('');
    setActiveRequestId(requestId);
    setLookupRequestId(requestId);
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

      {requestsQuery.isLoading ? (
        <Panel title="Loading privacy requests" subtitle="Retrieving recent requests submitted by you.">
          <div className="skeleton-grid">
            <div className="skeleton-block" />
            <div className="skeleton-block" />
          </div>
        </Panel>
      ) : null}

      {listError ? (
        <Panel title="Privacy request list failed" subtitle="We could not load your recent requests.">
          <div className="banner banner-error">{listError}</div>
        </Panel>
      ) : null}

      {!requestsQuery.isLoading && !listError ? (
        <>
          <Panel
            title="Recent workspace requests"
            subtitle={
              selectedCompany
                ? `Requests you submitted for ${selectedCompany.name}`
                : 'Select a workspace to see company-linked requests.'
            }
          >
            <PrivacyRequestTable
              emptyMessage={
                selectedCompany
                  ? 'No workspace privacy requests submitted by you yet.'
                  : 'Workspace requests appear once a company is selected.'
              }
              onView={handleViewRequest}
              requests={workspaceRequests}
            />
          </Panel>

          <Panel
            title="Recent personal requests"
            subtitle="Requests you submitted for your own account rather than a workspace."
          >
            <PrivacyRequestTable
              emptyMessage="No personal privacy requests submitted by you yet."
              onView={handleViewRequest}
              requests={personalRequests}
            />
          </Panel>
        </>
      ) : null}

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
