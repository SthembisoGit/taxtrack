export type UserRole = 'Owner' | 'Accountant' | 'FinanceManager' | 'Viewer';
export type DatasetType = 'transactions' | 'payroll' | 'vat_submissions';
export type UploadJobStatus = 'Queued' | 'Validating' | 'Processing' | 'Completed' | 'Failed';
export type RiskAnalysisJobStatus = 'Queued' | 'Processing' | 'Completed' | 'Failed';
export type RiskLevel = 'Low' | 'Medium' | 'High';
export type AlertSeverity = 'Info' | 'Warning' | 'Critical';
export type RuleClass = 'Regulatory' | 'Heuristic';

export interface AuthResponse {
  userId: string;
  email: string;
  role: UserRole;
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
}

export interface CompanyResponse {
  id: string;
  name: string;
  registrationNumber: string;
  industry: string;
  taxReference: string;
  ownerUserId: string;
  createdAt: string;
}

export interface UploadAcceptedResponse {
  uploadId: string;
  companyId: string;
  datasetType: DatasetType;
  status: UploadJobStatus;
  receivedAt: string;
}

export interface UploadStatusResponse {
  uploadId: string;
  companyId: string;
  status: UploadJobStatus;
  acceptedRows: number;
  rejectedRows: number;
  evidenceCompleteness: number;
  insufficientEvidence: boolean;
  updatedAt: string;
}

export interface AnalyzeAcceptedResponse {
  analysisId: string;
  companyId: string;
  status: RiskAnalysisJobStatus;
  queuedAt: string;
}

export interface RiskAnalysisJobStatusResponse {
  analysisId: string;
  companyId: string;
  status: RiskAnalysisJobStatus;
  resultId?: string | null;
  updatedAt: string;
}

export interface RiskAlert {
  ruleCode: string;
  ruleClass: RuleClass;
  type: string;
  description: string;
  severity: AlertSeverity;
  recommendation: string;
  evidenceJson: string;
}

export interface RiskResultResponse {
  resultId: string;
  companyId: string;
  riskScore: number;
  regulatoryScore: number;
  heuristicScore: number;
  riskLevel: RiskLevel;
  taxPolicyVersion: string;
  policyEffectiveDate: string;
  evidenceCompleteness: number;
  insufficientEvidence: boolean;
  alerts: RiskAlert[];
  generatedAt: string;
}

export interface ReportDownloadMetadata {
  format: string;
  url: string;
  expiresAt: string;
}

export interface ReportResponse {
  companyId: string;
  reportId: string;
  generatedAt: string;
  riskSummary: RiskResultResponse;
  alerts: RiskAlert[];
  downloadOptions: ReportDownloadMetadata[];
}

export interface ValidationIssue {
  rowNumber: number;
  columnName: string;
  errorCode: string;
  message: string;
}

export interface NormalizedProblem {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string;
  validationIssues: ValidationIssue[];
  fieldErrors: Record<string, string[]>;
}
