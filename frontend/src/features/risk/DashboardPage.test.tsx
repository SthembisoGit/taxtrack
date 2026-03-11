import { screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { DashboardPage } from '@/features/risk/DashboardPage';
import { apiClient } from '@/lib/api/client';
import { clearSession, saveSession, toAppSession } from '@/lib/auth/session';
import type { AuthResponse, RiskResultResponse } from '@/lib/api/types';
import { renderWithProviders } from '@/test/render';

const authResponse: AuthResponse = {
  userId: 'user-1',
  email: 'owner@taxtrack.co.za',
  role: 'Owner',
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  accessTokenExpiresAtUtc: '2026-03-11T10:00:00Z',
  refreshTokenExpiresAtUtc: '2026-03-18T10:00:00Z',
};

const riskResult: RiskResultResponse = {
  resultId: 'result-1',
  companyId: 'company-1',
  riskScore: 74,
  regulatoryScore: 52,
  heuristicScore: 22,
  riskLevel: 'High',
  taxPolicyVersion: 'za-2026.1',
  policyEffectiveDate: '2026-03-01',
  evidenceCompleteness: 88,
  insufficientEvidence: false,
  alerts: [
    {
      ruleCode: 'PAYE-REG-001',
      ruleClass: 'Regulatory',
      type: 'PayeReconciliationMismatch',
      description: 'Declared PAYE does not reconcile to payroll totals.',
      severity: 'Critical',
      recommendation: 'Reconcile EMP201 and payroll totals before the next filing.',
      evidenceJson: '{}',
    },
  ],
  generatedAtUtc: '2026-03-11T10:05:00Z',
};

describe('DashboardPage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it('renders the latest score and risk state', async () => {
    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    vi.spyOn(apiClient, 'getLatestRisk').mockResolvedValue(riskResult);

    renderWithProviders(<DashboardPage />);

    expect(await screen.findByText('74')).toBeTruthy();
    expect(screen.getByText('High').className).toContain('badge-danger');
    expect(screen.getByText('PAYE-REG-001')).toBeTruthy();
  });
});
