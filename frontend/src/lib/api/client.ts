import {
  clearSession,
  getSession,
  saveSession,
  toAppSession,
  type AppSession,
} from '@/lib/auth/session';
import { resetAppQueryClient } from '@/app/queryClient';
import { normalizeProblem } from '@/lib/utils/problem';
import type {
  AuditLogEventResponse,
  AnalyzeAcceptedResponse,
  AuthResponse,
  CompanyResponse,
  CreateDataSubjectRequestInput,
  DataSubjectRequestResponse,
  DatasetType,
  NormalizedProblem,
  ReportResponse,
  RiskAnalysisJobStatusResponse,
  RiskResultResponse,
  UploadAcceptedResponse,
  UploadStatusResponse,
} from '@/lib/api/types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

export class ApiError extends Error {
  readonly problem: NormalizedProblem;

  constructor(problem: NormalizedProblem) {
    super(problem.detail);
    this.name = 'ApiError';
    this.problem = problem;
  }
}

export class SessionExpiredError extends Error {
  constructor() {
    super('Your session expired. Please sign in again.');
    this.name = 'SessionExpiredError';
  }
}

let refreshPromise: Promise<AppSession | null> | null = null;

function isAbsoluteUrl(path: string) {
  return /^https?:\/\//i.test(path);
}

function buildUrl(path: string) {
  if (isAbsoluteUrl(path)) {
    return path;
  }

  return `${API_BASE_URL}${path}`;
}

export function resolveApiUrl(path: string) {
  return buildUrl(path);
}

function isJsonContentType(contentType: string | null) {
  if (!contentType) {
    return false;
  }

  return contentType.includes('application/json') || contentType.includes('+json');
}

async function parseResponse<T>(response: Response) {
  const hasJson = isJsonContentType(response.headers.get('content-type'));
  if (!hasJson) {
    return null as T;
  }

  return (await response.json()) as T;
}

async function refreshSession() {
  if (refreshPromise) {
    return refreshPromise;
  }

  const session = getSession();
  if (!session?.refreshToken) {
    clearSession();
    resetAppQueryClient();
    return null;
  }

  refreshPromise = (async () => {
    try {
      const response = await fetch(buildUrl('/api/auth/refresh'), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken: session.refreshToken }),
      });

      if (!response.ok) {
        clearSession();
        resetAppQueryClient();
        return null;
      }

      const payload = await parseResponse<AuthResponse>(response);
      if (!payload) {
        clearSession();
        resetAppQueryClient();
        return null;
      }

      const nextSession = toAppSession(payload, session.selectedCompany);
      saveSession(nextSession);
      return nextSession;
    } catch {
      clearSession();
      resetAppQueryClient();
      return null;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

interface RequestOptions extends RequestInit {
  auth?: boolean;
  retryOnUnauthorized?: boolean;
}

async function sendRequest(path: string, options: RequestOptions = {}): Promise<Response> {
  const { auth = true, retryOnUnauthorized = true, headers, ...init } = options;
  const session = getSession();
  const requestHeaders = new Headers(headers);

  if (auth && session?.accessToken) {
    requestHeaders.set('Authorization', `Bearer ${session.accessToken}`);
  }

  const response = await fetch(buildUrl(path), {
    ...init,
    headers: requestHeaders,
  });

  if (response.status === 401 && auth && retryOnUnauthorized) {
    const nextSession = await refreshSession();
    if (!nextSession) {
      throw new SessionExpiredError();
    }

    return sendRequest(path, {
      ...options,
      retryOnUnauthorized: false,
    });
  }

  return response;
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const response = await sendRequest(path, options);

  if (!response.ok) {
    const payload = await parseResponse<unknown>(response);
    throw new ApiError(normalizeProblem(payload, response.status));
  }

  const payload = await parseResponse<T>(response);
  return payload;
}

function extractFileName(contentDisposition: string | null) {
  if (!contentDisposition) {
    return 'taxtrack-report.json';
  }

  const utf8Match = contentDisposition.match(/filename\*=UTF-8''([^;]+)/i);
  if (utf8Match?.[1]) {
    return decodeURIComponent(utf8Match[1]);
  }

  const quotedMatch = contentDisposition.match(/filename="([^"]+)"/i);
  if (quotedMatch?.[1]) {
    return quotedMatch[1];
  }

  const plainMatch = contentDisposition.match(/filename=([^;]+)/i);
  if (plainMatch?.[1]) {
    return plainMatch[1].trim();
  }

  return 'taxtrack-report.json';
}

export const apiClient = {
  register(input: { email: string; password: string; role: string }) {
    return request<unknown>('/api/auth/register', {
      auth: false,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    });
  },
  login(input: { email: string; password: string }) {
    return request<AuthResponse>('/api/auth/login', {
      auth: false,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    });
  },
  createCompany(input: {
    name: string;
    registrationNumber: string;
    industry: string;
    taxReference: string;
  }) {
    return request<CompanyResponse>('/api/company', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    });
  },
  listCompanies() {
    return request<CompanyResponse[]>('/api/company');
  },
  uploadFinancial(input: {
    companyId: string;
    datasetType: DatasetType;
    file: File;
    idempotencyKey: string;
  }) {
    const formData = new FormData();
    formData.append('CompanyId', input.companyId);
    formData.append('DatasetType', input.datasetType);
    formData.append('File', input.file);

    return request<UploadAcceptedResponse>('/api/financial/upload', {
      method: 'POST',
      headers: {
        'Idempotency-Key': input.idempotencyKey,
      },
      body: formData,
    });
  },
  getUploadStatus(uploadId: string) {
    return request<UploadStatusResponse>(`/api/financial/upload/${uploadId}/status`);
  },
  analyzeRisk(input: {
    companyId: string;
    periodStart?: string;
    periodEnd?: string;
    idempotencyKey: string;
  }) {
    return request<AnalyzeAcceptedResponse>('/api/risk/analyze', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Idempotency-Key': input.idempotencyKey,
      },
      body: JSON.stringify({
        companyId: input.companyId,
        periodStart: input.periodStart,
        periodEnd: input.periodEnd,
      }),
    });
  },
  getAnalysisStatus(analysisId: string) {
    return request<RiskAnalysisJobStatusResponse>(`/api/risk/analyze/${analysisId}/status`);
  },
  getLatestRisk(companyId: string) {
    return request<RiskResultResponse>(`/api/risk/${companyId}`);
  },
  getReport(companyId: string) {
    return request<ReportResponse>(`/api/report/${companyId}`);
  },
  async downloadReport(downloadUrl: string) {
    const response = await sendRequest(downloadUrl);
    if (!response.ok) {
      const payload = await parseResponse<unknown>(response);
      throw new ApiError(normalizeProblem(payload, response.status));
    }

    return {
      blob: await response.blob(),
      fileName: extractFileName(response.headers.get('content-disposition')),
    };
  },
  getAuditLog(input: { companyId?: string; limit?: number }) {
    const params = new URLSearchParams();
    if (input.companyId) {
      params.set('companyId', input.companyId);
    }
    if (input.limit) {
      params.set('limit', String(input.limit));
    }

    const suffix = params.size ? `?${params.toString()}` : '';
    return request<AuditLogEventResponse[]>(`/api/audit${suffix}`);
  },
  createDataRequest(input: CreateDataSubjectRequestInput) {
    return request<DataSubjectRequestResponse>('/api/privacy/data-requests', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        companyId: input.companyId,
        requestType: input.requestType,
        reason: input.reason,
      }),
    });
  },
  listDataRequests() {
    return request<DataSubjectRequestResponse[]>('/api/privacy/data-requests');
  },
  getDataRequest(requestId: string) {
    return request<DataSubjectRequestResponse>(`/api/privacy/data-requests/${requestId}`);
  },
};
