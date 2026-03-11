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

const requestResponse: DataSubjectRequestResponse = {
  requestId: '3f0bc5a3-6702-4db1-a815-4db4882d4b6f',
  requestType: 'Export',
  status: 'Received',
  createdAtUtc: '2026-03-11T10:05:00Z',
  updatedAtUtc: '2026-03-11T10:05:00Z',
  resolutionNote: null,
};

describe('PrivacyPage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it('submits a workspace export request and renders the returned status', async () => {
    const user = userEvent.setup();

    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    const createSpy = vi.spyOn(apiClient, 'createDataRequest').mockResolvedValue(requestResponse);
    const getSpy = vi.spyOn(apiClient, 'getDataRequest').mockResolvedValue(requestResponse);

    renderWithProviders(<PrivacyPage />);

    await user.selectOptions(screen.getByLabelText('Request type'), 'Export');
    await user.type(
      screen.getByPlaceholderText('Example: Customer requested a copy of submitted financial data.'),
      'Customer requested a copy of submitted records.',
    );
    await user.click(screen.getByRole('button', { name: 'Submit privacy request' }));

    await waitFor(() => {
      expect(createSpy).toHaveBeenCalledWith({
        companyId: 'company-1',
        requestType: 'Export',
        reason: 'Customer requested a copy of submitted records.',
      });
      expect(getSpy).toHaveBeenCalledWith(requestResponse.requestId);
    });

    expect(await screen.findByText('Request status')).toBeTruthy();
    expect(screen.getByText(requestResponse.requestId)).toBeTruthy();
    expect(screen.getByText('Received')).toBeTruthy();
  }, 15000);
});
