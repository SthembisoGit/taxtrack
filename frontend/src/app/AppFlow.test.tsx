import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError, apiClient } from '@/lib/api/client';
import type {
  AnalyzeAcceptedResponse,
  AuthResponse,
  CompanyResponse,
  ReportResponse,
  RiskResultResponse,
  UploadAcceptedResponse,
  UploadStatusResponse,
} from '@/lib/api/types';
import { clearSession } from '@/lib/auth/session';
import { renderWithAppRouter } from '@/test/render';

const authResponse: AuthResponse = {
  userId: 'user-1',
  email: 'owner@taxtrack.co.za',
  role: 'Owner',
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  accessTokenExpiresAtUtc: '2026-03-11T10:00:00Z',
  refreshTokenExpiresAtUtc: '2026-03-18T10:00:00Z',
};

const company: CompanyResponse = {
  id: 'company-1',
  name: 'Acme Holdings',
  registrationNumber: '2018/123456/07',
  industry: 'Technology',
  taxReference: '1234567890',
  ownerUserId: 'user-1',
  createdAtUtc: '2026-03-11T10:01:00Z',
};

const uploadAccepted: UploadAcceptedResponse = {
  uploadId: 'upload-1',
  companyId: company.id,
  datasetType: 'transactions',
  status: 'Queued',
  receivedAtUtc: '2026-03-11T10:03:00Z',
};

const uploadStatus: UploadStatusResponse = {
  uploadId: 'upload-1',
  companyId: company.id,
  status: 'Completed',
  acceptedRows: 18,
  rejectedRows: 0,
  evidenceCompleteness: 94,
  insufficientEvidence: false,
  updatedAtUtc: '2026-03-11T10:03:30Z',
};

const analyzeAccepted: AnalyzeAcceptedResponse = {
  analysisId: 'analysis-1',
  companyId: company.id,
  status: 'Queued',
  queuedAtUtc: '2026-03-11T10:05:00Z',
};

const riskResult: RiskResultResponse = {
  resultId: 'result-1',
  companyId: company.id,
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
  generatedAtUtc: '2026-03-11T10:06:00Z',
};

const reportResponse: ReportResponse = {
  companyId: company.id,
  reportId: 'report-1',
  generatedAtUtc: '2026-03-11T10:07:00Z',
  riskSummary: riskResult,
  alerts: riskResult.alerts,
  downloadOptions: [
    {
      format: 'json',
      url: 'https://example.com/report.json',
      expiresAtUtc: '2026-03-11T11:07:00Z',
    },
  ],
};

describe('App flow', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it(
    'supports login, workspace creation, upload, analysis, and report review in one route flow',
    async () => {
      const user = userEvent.setup();
      let companies: CompanyResponse[] = [];
      let analysisCompleted = false;

      vi.spyOn(apiClient, 'login').mockResolvedValue(authResponse);
      vi.spyOn(apiClient, 'listCompanies').mockImplementation(async () => companies);
      vi.spyOn(apiClient, 'createCompany').mockImplementation(async () => {
        companies = [company];
        return company;
      });
      const uploadFinancialSpy = vi.spyOn(apiClient, 'uploadFinancial').mockResolvedValue(uploadAccepted);
      const getUploadStatusSpy = vi.spyOn(apiClient, 'getUploadStatus').mockResolvedValue(uploadStatus);
      const analyzeRiskSpy = vi.spyOn(apiClient, 'analyzeRisk').mockResolvedValue(analyzeAccepted);
      const getAnalysisStatusSpy = vi.spyOn(apiClient, 'getAnalysisStatus').mockImplementation(async () => {
        analysisCompleted = true;
        return {
          analysisId: analyzeAccepted.analysisId,
          companyId: company.id,
          status: 'Completed',
          resultId: riskResult.resultId,
          updatedAtUtc: '2026-03-11T10:06:00Z',
        };
      });
      vi.spyOn(apiClient, 'getLatestRisk').mockImplementation(async () => {
        if (!analysisCompleted) {
          throw new ApiError({
            type: 'about:blank',
            title: 'Not Found',
            status: 404,
            detail: 'No completed risk analysis exists for this company.',
            instance: `/api/risk/${company.id}`,
            validationIssues: [],
            fieldErrors: {},
          });
        }

        return riskResult;
      });
      vi.spyOn(apiClient, 'getReport').mockResolvedValue(reportResponse);

      renderWithAppRouter({ initialEntries: ['/auth'] });

      await user.type(screen.getByLabelText('Email'), authResponse.email);
      await user.type(screen.getByLabelText('Password'), 'supersecure123');
      await user.click(screen.getByRole('button', { name: 'Access workspace' }));

      expect(
        await screen.findByText('Choose an existing workspace or register a new company.'),
      ).toBeTruthy();

      await user.type(screen.getByLabelText('Company name'), company.name);
      await user.type(screen.getByLabelText('Registration number'), company.registrationNumber);
      await user.selectOptions(screen.getByLabelText('Industry'), company.industry);
      await user.type(screen.getByLabelText('Tax reference'), company.taxReference);
      await user.click(screen.getByRole('button', { name: 'Save company' }));

      expect(await screen.findByText('Upload the datasets that power the next risk analysis.')).toBeTruthy();

      const file = new File(
        ['date,category,amount,source\n2026-03-01,Revenue,1200.00,ERP'],
        'transactions.csv',
        { type: 'text/csv' },
      );

      const fileInput = document.querySelector('input[type="file"]');
      expect(fileInput).toBeTruthy();
      await user.upload(fileInput as HTMLInputElement, file);
      await user.click(screen.getByRole('button', { name: 'Upload dataset' }));

      await waitFor(() => {
        expect(uploadFinancialSpy).toHaveBeenCalledOnce();
        expect(getUploadStatusSpy).toHaveBeenCalledOnce();
      });

      await user.click(screen.getByRole('link', { name: 'Dashboard' }));

      expect(await screen.findByText('No analysis yet')).toBeTruthy();

      await user.click(screen.getByRole('button', { name: 'Run analysis' }));
      await waitFor(() => {
        expect(analyzeRiskSpy).toHaveBeenCalledOnce();
        expect(getAnalysisStatusSpy).toHaveBeenCalledOnce();
      });

      expect(await screen.findByText('74')).toBeTruthy();
      expect(screen.getByText('PAYE-REG-001')).toBeTruthy();

      await user.click(screen.getByRole('link', { name: 'Report' }));

      expect(await screen.findByText('Report summary')).toBeTruthy();
      expect(screen.getByText('74')).toBeTruthy();
      expect(
        screen.getByText('Report summary and alert output match the latest dashboard analysis result.'),
      ).toBeTruthy();

      await waitFor(() => {
        expect(screen.getByRole('link', { name: /json/i }).getAttribute('href')).toBe(
          'https://example.com/report.json',
        );
      });
    },
    45000,
  );
});
