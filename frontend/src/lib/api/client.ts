import {
  clearSession,
  getSession,
  saveSession,
  toAppSession,
  type AppSession,
} from '@/lib/auth/session';
import { normalizeProblem } from '@/lib/utils/problem';
import type {
  AnalyzeAcceptedResponse,
  AuthResponse,
  CompanyResponse,
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

function buildUrl(path: string) {
  return `${API_BASE_URL}${path}`;
}

async function parseResponse<T>(response: Response) {
  const hasJson = response.headers.get('content-type')?.includes('application/json');
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
        return null;
      }

      const payload = await parseResponse<AuthResponse>(response);
      if (!payload) {
        clearSession();
        return null;
      }

      const nextSession = toAppSession(payload, session.selectedCompany);
      saveSession(nextSession);
      return nextSession;
    } catch {
      clearSession();
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

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
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

    return request<T>(path, {
      ...options,
      retryOnUnauthorized: false,
    });
  }

  if (!response.ok) {
    const payload = await parseResponse<unknown>(response);
    throw new ApiError(normalizeProblem(payload, response.status));
  }

  const payload = await parseResponse<T>(response);
  return payload;
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
};
