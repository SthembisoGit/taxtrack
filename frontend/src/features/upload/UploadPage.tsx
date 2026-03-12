import { useRef, useState, type ChangeEvent, type DragEvent, type KeyboardEvent } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { Panel } from '@/components/ui/Panel';
import { SelectField, TextAreaField } from '@/components/ui/FormField';
import { apiClient, ApiError, SessionExpiredError } from '@/lib/api/client';
import { useAuthSession } from '@/lib/auth/session';
import { generateIdempotencyKey } from '@/lib/utils/idempotency';
import type { DatasetType, ValidationIssue } from '@/lib/api/types';

const datasets: { label: string; value: DatasetType }[] = [
  { label: 'Transactions', value: 'transactions' },
  { label: 'Payroll', value: 'payroll' },
  { label: 'VAT submissions', value: 'vat_submissions' },
];

function getUploadErrorMessage(caught: unknown) {
  if (caught instanceof ApiError) {
    return caught.problem.detail;
  }

  if (caught instanceof SessionExpiredError) {
    return caught.message;
  }

  if (caught instanceof TypeError) {
    return 'Could not reach the TaxTrack API. Check that the backend is running and the frontend API URL is correct.';
  }

  if (caught instanceof Error && caught.message) {
    return caught.message;
  }

  return 'We could not process that upload.';
}

export function UploadPage() {
  const session = useAuthSession();
  const companyId = session?.selectedCompany?.id;

  if (!companyId) {
    return (
      <Panel title="No company selected" subtitle="Set up a company profile before uploading datasets.">
        <p className="empty-copy">TaxTrack needs a company context to validate and score uploaded data.</p>
      </Panel>
    );
  }

  return <UploadWorkspace key={companyId} companyId={companyId} companyName={session.selectedCompany?.name ?? ''} />;
}

interface UploadWorkspaceProps {
  companyId: string;
  companyName: string;
}

function UploadWorkspace({ companyId, companyName }: UploadWorkspaceProps) {
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [datasetType, setDatasetType] = useState<DatasetType>('transactions');
  const [file, setFile] = useState<File | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [notes, setNotes] = useState('');
  const [lastUploadId, setLastUploadId] = useState('');
  const [validationMessage, setValidationMessage] = useState('');
  const [validationIssues, setValidationIssues] = useState<ValidationIssue[]>([]);

  function setSelectedFile(nextFile: File | null) {
    setFile(nextFile);
  }

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    setSelectedFile(event.target.files?.[0] ?? null);
  }

  function openFilePicker() {
    fileInputRef.current?.click();
  }

  function handleDragEnter(event: DragEvent<HTMLDivElement>) {
    event.preventDefault();
    setIsDragging(true);
  }

  function handleDragLeave(event: DragEvent<HTMLDivElement>) {
    event.preventDefault();
    setIsDragging(false);
  }

  function handleDragOver(event: DragEvent<HTMLDivElement>) {
    event.preventDefault();
  }

  function handleDrop(event: DragEvent<HTMLDivElement>) {
    event.preventDefault();
    setIsDragging(false);
    setSelectedFile(event.dataTransfer.files?.[0] ?? null);
  }

  function handleDropzoneKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      openFilePicker();
    }
  }

  const uploadMutation = useMutation({
    mutationFn: async () => {
      if (!companyId || !file) {
        throw new Error('A company and a file are required.');
      }

      return apiClient.uploadFinancial({
        companyId,
        datasetType,
        file,
        idempotencyKey: generateIdempotencyKey(),
      });
    },
    onSuccess: (response) => {
      setValidationMessage('');
      setValidationIssues([]);
      setLastUploadId(response.uploadId);
    },
    onError: (caught) => {
      if (caught instanceof ApiError) {
        setValidationMessage(caught.problem.detail);
        setValidationIssues(caught.problem.validationIssues);
        return;
      }

      setValidationIssues([]);
      setValidationMessage(getUploadErrorMessage(caught));
    },
  });

  const uploadStatusQuery = useQuery({
    queryKey: ['upload-status', lastUploadId],
    enabled: Boolean(lastUploadId),
    queryFn: () => apiClient.getUploadStatus(lastUploadId),
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      if (!status || status === 'Completed' || status === 'Failed') {
        return false;
      }

      return 2000;
    },
  });

  return (
    <div className="stack gap-lg">
      <div className="page-heading">
        <p className="eyebrow">CSV intake</p>
        <h1>Upload the datasets that power the next risk analysis.</h1>
      </div>

      <Panel title="Upload dataset" subtitle={`Working company: ${companyName}`}>
        <form
          className="stack"
          noValidate
          onSubmit={(event) => {
            event.preventDefault();
            uploadMutation.mutate();
          }}
        >
          <div className="upload-grid">
            <SelectField
              label="Dataset type"
              onChange={(event) => setDatasetType(event.target.value as DatasetType)}
              value={datasetType}
            >
              {datasets.map((item) => (
                <option key={item.value} value={item.value}>
                  {item.label}
                </option>
              ))}
            </SelectField>

            <label className="field">
              <span className="field-label">CSV file</span>
              <input
                accept=".csv,text/csv"
                className="field-input file-input"
                onChange={handleFileChange}
                ref={fileInputRef}
                required
                type="file"
              />
              <span className="field-helper">TaxTrack accepts structured CSV only for this slice.</span>
            </label>
          </div>

          <TextAreaField
            helperText="Optional handoff notes for accountants or finance managers."
            label="Upload notes"
            onChange={(event) => setNotes(event.target.value)}
            placeholder="Optional context for the team reviewing this upload."
            value={notes}
          />

          <div
            aria-label="Drag a CSV here or browse to select one."
            aria-live="polite"
            className={isDragging ? 'upload-dropzone is-dragging' : 'upload-dropzone'}
            onClick={openFilePicker}
            onDragEnter={handleDragEnter}
            onDragLeave={handleDragLeave}
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            onKeyDown={handleDropzoneKeyDown}
            role="button"
            tabIndex={0}
          >
            <p className="dropzone-title">{file ? file.name : 'Drag a CSV here or browse to select one.'}</p>
            <p className="dropzone-copy">
              {file
                ? `${Math.round(file.size / 1024)} KB selected | ${datasetType}`
                : 'Files are validated against the v1 data contract before persistence.'}
            </p>
          </div>

          <div className="button-row">
            <button className="button button-primary" disabled={uploadMutation.isPending || !file} type="submit">
              {uploadMutation.isPending ? 'Uploading...' : 'Upload dataset'}
            </button>
          </div>
        </form>
      </Panel>

      {validationMessage ? (
        <Panel title="Validation feedback" subtitle="Fix the highlighted issues and re-upload the file.">
          <div className="banner banner-error">{validationMessage}</div>
          {validationIssues.length ? (
            <div className="table-wrap">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Row</th>
                    <th>Column</th>
                    <th>Code</th>
                    <th>Message</th>
                  </tr>
                </thead>
                <tbody>
                  {validationIssues.map((issue, index) => (
                    <tr key={`${issue.rowNumber}-${issue.columnName}-${index}`}>
                      <td>{issue.rowNumber}</td>
                      <td>{issue.columnName}</td>
                      <td>{issue.errorCode}</td>
                      <td>{issue.message}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : null}
        </Panel>
      ) : null}

      {uploadStatusQuery.data ? (
        <Panel title="Latest upload status" subtitle={`Upload ID ${uploadStatusQuery.data.uploadId}`}>
          <div className="summary-grid">
            <div className="summary-metric">
              <span className="metric-label">Status</span>
              <strong>{uploadStatusQuery.data.status}</strong>
            </div>
            <div className="summary-metric">
              <span className="metric-label">Accepted rows</span>
              <strong>{uploadStatusQuery.data.acceptedRows}</strong>
            </div>
            <div className="summary-metric">
              <span className="metric-label">Rejected rows</span>
              <strong>{uploadStatusQuery.data.rejectedRows}</strong>
            </div>
            <div className="summary-metric">
              <span className="metric-label">Evidence completeness</span>
              <strong>{uploadStatusQuery.data.evidenceCompleteness}%</strong>
            </div>
          </div>
        </Panel>
      ) : null}
    </div>
  );
}
