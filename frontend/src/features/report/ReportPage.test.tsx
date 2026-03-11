import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ReportPage } from '@/features/report/ReportPage';
import { apiClient, ApiError } from '@/lib/api/client';
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

const matchingLatestRisk = {
  ...reportResponse.riskSummary,
  alerts: reportResponse.alerts,
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
    vi.spyOn(apiClient, 'getLatestRisk').mockResolvedValue(matchingLatestRisk);

    renderWithProviders(<ReportPage />);

    expect(await screen.findByText('Report summary')).toBeTruthy();
    expect(screen.getByText('61')).toBeTruthy();
    expect(
      screen.getByText('Report summary and alert output match the latest dashboard analysis result.'),
    ).toBeTruthy();
    expect(screen.getByRole('link', { name: /json/i }).getAttribute('href')).toBe(
      'https://example.com/report.json',
    );
  }, 15000);

  it('shows a warning when the report drifts from the latest dashboard result', async () => {
    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    vi.spyOn(apiClient, 'getReport').mockResolvedValue(reportResponse);
    vi.spyOn(apiClient, 'getLatestRisk').mockResolvedValue({
      ...matchingLatestRisk,
      resultId: 'result-2',
      riskScore: 74,
      riskLevel: 'High',
      alerts: [
        {
          ruleCode: 'VAT-HEU-004',
          ruleClass: 'Heuristic',
          type: 'VatRefundSpike',
          description: 'VAT refund trend is unusually high for the reported turnover.',
          severity: 'Warning',
          recommendation: 'Validate refund claims and supporting invoices before submission.',
          evidenceJson: '{}',
        },
      ],
    });

    renderWithProviders(<ReportPage />);

    expect(await screen.findByText('Consistency issues detected.')).toBeTruthy();
    expect(
      screen.getByText('Report snapshot is not using the latest completed analysis result.'),
    ).toBeTruthy();
    expect(
      screen.getByText('Report score or risk level differs from the latest dashboard result.'),
    ).toBeTruthy();
    expect(screen.getByText('Report alerts differ from the latest dashboard result.')).toBeTruthy();
  });

  it('retries report retrieval after an initial failure', async () => {
    const user = userEvent.setup();

    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    let reportAttempts = 0;
    vi.spyOn(apiClient, 'getReport').mockImplementation(async () => {
      reportAttempts += 1;
      if (reportAttempts === 1) {
        throw new ApiError({
          type: 'about:blank',
          title: 'Request failed',
          status: 503,
          detail: 'Report service is temporarily unavailable.',
          instance: '/api/report/company-1',
          validationIssues: [],
          fieldErrors: {},
        });
      }

      return reportResponse;
    });
    vi.spyOn(apiClient, 'getLatestRisk').mockResolvedValue(matchingLatestRisk);

    renderWithProviders(<ReportPage />);

    expect(await screen.findByText('Report service is temporarily unavailable.')).toBeTruthy();

    await user.click(screen.getByRole('button', { name: 'Try again' }));

    expect(await screen.findByText('Report summary')).toBeTruthy();
    expect(reportAttempts).toBe(2);
  }, 15000);
});
