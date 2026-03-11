import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { PrivacyPage } from '@/features/privacy/PrivacyPage';
import { apiClient } from '@/lib/api/client';
import { clearSession, saveSession, toAppSession } from '@/lib/auth/session';
import type { AuthResponse, DataSubjectRequestResponse } from '@/lib/api/types';
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

const existingWorkspaceRequest: DataSubjectRequestResponse = {
  requestId: '3f0bc5a3-6702-4db1-a815-4db4882d4b6f',
  companyId: 'company-1',
  requestType: 'Export',
  status: 'Completed',
  reason: 'Customer requested a copy of submitted records.',
  createdAtUtc: '2026-03-11T10:05:00Z',
  updatedAtUtc: '2026-03-11T10:15:00Z',
  resolutionNote: 'Export package delivered securely.',
};

const existingPersonalRequest: DataSubjectRequestResponse = {
  requestId: '10d1d775-93fb-4ec4-87f1-df3d131cb5b1',
  companyId: null,
  requestType: 'Deletion',
  status: 'Received',
  reason: 'Remove personal account profile after pilot.',
  createdAtUtc: '2026-03-11T08:00:00Z',
  updatedAtUtc: '2026-03-11T08:00:00Z',
  resolutionNote: null,
};

const createdRequest: DataSubjectRequestResponse = {
  requestId: 'af6d4e8f-0f78-46ef-9bb8-fcb7c7c2ce78',
  companyId: 'company-1',
  requestType: 'Export',
  status: 'Received',
  reason: 'Board requested current tax submission package.',
  createdAtUtc: '2026-03-11T11:05:00Z',
  updatedAtUtc: '2026-03-11T11:05:00Z',
  resolutionNote: null,
};

describe('PrivacyPage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it('renders recent workspace and personal requests', async () => {
    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    vi.spyOn(apiClient, 'listDataRequests').mockResolvedValue([
      existingWorkspaceRequest,
      existingPersonalRequest,
    ]);

    renderWithProviders(<PrivacyPage />);

    expect(await screen.findByText(existingWorkspaceRequest.requestId)).toBeTruthy();
    expect(screen.getByText(existingPersonalRequest.requestId)).toBeTruthy();
    expect(screen.getByText(existingWorkspaceRequest.reason ?? '')).toBeTruthy();
  }, 15000);

  it('submits a workspace export request and renders the returned status', async () => {
    const user = userEvent.setup();

    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    const listSpy = vi.spyOn(apiClient, 'listDataRequests').mockResolvedValue([
      existingWorkspaceRequest,
      existingPersonalRequest,
    ]);
    const createSpy = vi.spyOn(apiClient, 'createDataRequest').mockResolvedValue(createdRequest);
    const getSpy = vi.spyOn(apiClient, 'getDataRequest').mockResolvedValue(createdRequest);

    renderWithProviders(<PrivacyPage />);

    await screen.findByText(existingWorkspaceRequest.requestId);

    await user.selectOptions(screen.getByLabelText('Request type'), 'Export');
    await user.type(
      screen.getByPlaceholderText('Example: Customer requested a copy of submitted financial data.'),
      createdRequest.reason ?? '',
    );
    await user.click(screen.getByRole('button', { name: 'Submit privacy request' }));

    await waitFor(() => {
      expect(listSpy).toHaveBeenCalled();
      expect(createSpy).toHaveBeenCalledWith({
        companyId: 'company-1',
        requestType: 'Export',
        reason: createdRequest.reason,
      });
    });

    expect(await screen.findAllByText(createdRequest.requestId)).toBeTruthy();
    expect(screen.getAllByText(createdRequest.reason ?? '').length).toBeGreaterThan(0);
    await waitFor(() => {
      expect(getSpy).toHaveBeenCalledWith(createdRequest.requestId);
    });
  }, 15000);
});
