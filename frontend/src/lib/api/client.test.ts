import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient, SessionExpiredError } from '@/lib/api/client';
import { clearSession, getSession, saveSession, toAppSession } from '@/lib/auth/session';
import type { AuthResponse, RiskResultResponse } from '@/lib/api/types';

const staleAuth: AuthResponse = {
  userId: 'user-1',
  email: 'owner@taxtrack.co.za',
  role: 'Owner',
  accessToken: 'stale-access-token',
  refreshToken: 'valid-refresh-token',
  accessTokenExpiresAtUtc: '2026-03-11T10:00:00Z',
  refreshTokenExpiresAtUtc: '2026-03-18T10:00:00Z',
};

const refreshedAuth: AuthResponse = {
  ...staleAuth,
  accessToken: 'fresh-access-token',
  refreshToken: 'rotated-refresh-token',
  accessTokenExpiresAtUtc: '2026-03-11T11:00:00Z',
  refreshTokenExpiresAtUtc: '2026-03-18T11:00:00Z',
};

const riskResult: RiskResultResponse = {
  resultId: 'result-1',
  companyId: 'company-1',
  riskScore: 61,
  regulatoryScore: 45,
  heuristicScore: 16,
  riskLevel: 'Medium',
  taxPolicyVersion: 'za-2026.1',
  policyEffectiveDate: '2026-03-01',
  evidenceCompleteness: 92,
  insufficientEvidence: false,
  alerts: [
    {
      ruleCode: 'VAT-HEUR-001',
      ruleClass: 'Heuristic',
      type: 'VatRefundSpike',
      description: 'VAT refund ratio is above the configured tolerance.',
      severity: 'Warning',
      recommendation: 'Review source tax invoices before filing.',
      evidenceJson: '{}',
    },
  ],
  generatedAtUtc: '2026-03-11T10:05:00Z',
};

function jsonResponse(payload: unknown, status = 200) {
  return new Response(JSON.stringify(payload), {
    status,
    headers: {
      'Content-Type': 'application/json',
    },
  });
}

describe('apiClient refresh handling', () => {
  beforeEach(() => {
    window.sessionStorage.clear();
    clearSession();
    saveSession(toAppSession(staleAuth));
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    clearSession();
    window.sessionStorage.clear();
  });

  it('refreshes once after a 401 and retries the protected request', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(jsonResponse(refreshedAuth))
      .mockResolvedValueOnce(jsonResponse(riskResult));

    vi.stubGlobal('fetch', fetchMock);

    await expect(apiClient.getLatestRisk('company-1')).resolves.toEqual(riskResult);
    expect(fetchMock).toHaveBeenCalledTimes(3);

    const retriedHeaders = new Headers(fetchMock.mock.calls[2]?.[1]?.headers as HeadersInit);
    expect(retriedHeaders.get('Authorization')).toBe('Bearer fresh-access-token');
    expect(getSession()?.accessToken).toBe('fresh-access-token');
    expect(getSession()?.refreshToken).toBe('rotated-refresh-token');
  });

  it('clears the session when refresh fails', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(null, { status: 401 }));

    vi.stubGlobal('fetch', fetchMock);

    await expect(apiClient.getLatestRisk('company-1')).rejects.toBeInstanceOf(SessionExpiredError);
    expect(getSession()).toBeNull();
  });
});
