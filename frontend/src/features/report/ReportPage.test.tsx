import { screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ReportPage } from '@/features/report/ReportPage';
import { apiClient } from '@/lib/api/client';
import { clearSession, saveSession, toAppSession } from '@/lib/auth/session';
import type { AuthResponse, ReportResponse } from '@/lib/api/types';
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

const reportResponse: ReportResponse = {
  companyId: 'company-1',
  reportId: 'report-1',
  generatedAtUtc: '2026-03-11T10:05:00Z',
  riskSummary: {
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
    alerts: [],
    generatedAtUtc: '2026-03-11T10:05:00Z',
  },
  alerts: [],
  downloadOptions: [
    {
      format: 'json',
      url: 'https://example.com/report.json',
      expiresAtUtc: '2026-03-11T11:05:00Z',
    },
  ],
};

describe('ReportPage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it('renders report summary and download links', async () => {
    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    vi.spyOn(apiClient, 'getReport').mockResolvedValue(reportResponse);

    renderWithProviders(<ReportPage />);

    expect(await screen.findByText('Report summary')).toBeTruthy();
    expect(screen.getByText('61')).toBeTruthy();
    expect(screen.getByRole('link', { name: /json/i }).getAttribute('href')).toBe(
      'https://example.com/report.json',
    );
  }, 15000);
});
