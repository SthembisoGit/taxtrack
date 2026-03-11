import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { AuditPage } from '@/features/audit/AuditPage';
import { apiClient, ApiError } from '@/lib/api/client';
import { clearSession, saveSession, toAppSession } from '@/lib/auth/session';
import type { AuditLogEventResponse, AuthResponse } from '@/lib/api/types';
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

const companyEvents: AuditLogEventResponse[] = [
  {
    eventId: 'event-1',
    actorUserId: 'user-1',
    actorEmail: 'owner@taxtrack.co.za',
    companyId: 'company-1',
    eventType: 'UploadCreated',
    eventTimeUtc: '2026-03-11T10:00:00Z',
    correlationId: 'corr-upload',
    metadataJson: '{"uploadId":"upload-1"}',
    ipAddress: '127.0.0.1',
    userAgent: 'vitest',
  },
];

const securityEvents: AuditLogEventResponse[] = [
  {
    eventId: 'event-2',
    actorUserId: 'user-1',
    actorEmail: 'owner@taxtrack.co.za',
    companyId: null,
    eventType: 'LoginSucceeded',
    eventTimeUtc: '2026-03-11T09:55:00Z',
    correlationId: 'corr-login',
    metadataJson: '{"email":"owner@taxtrack.co.za"}',
    ipAddress: '127.0.0.1',
    userAgent: 'vitest',
  },
];

describe('AuditPage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it('renders workspace and personal audit events', async () => {
    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    const getAuditLogSpy = vi.spyOn(apiClient, 'getAuditLog').mockImplementation(async (input) =>
      input.companyId ? companyEvents : securityEvents,
    );

    renderWithProviders(<AuditPage />);

    await waitFor(() => {
      expect(getAuditLogSpy).toHaveBeenCalledTimes(2);
    }, { timeout: 5000 });

    expect(await screen.findByText('Upload created', {}, { timeout: 5000 })).toBeTruthy();
    expect(await screen.findByText('Login succeeded', {}, { timeout: 5000 })).toBeTruthy();
    expect(screen.getByText('corr-upload')).toBeTruthy();
    expect(screen.getByText('corr-login')).toBeTruthy();
  }, 15000);

  it('retries security audit retrieval after an initial failure', async () => {
    const user = userEvent.setup();

    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    let securityAttempts = 0;
    vi.spyOn(apiClient, 'getAuditLog').mockImplementation(async (input) => {
      if (input.companyId) {
        return companyEvents;
      }

      securityAttempts += 1;
      if (securityAttempts === 1) {
        throw new ApiError({
          type: 'about:blank',
          title: 'Request failed',
          status: 503,
          detail: 'Security audit service is temporarily unavailable.',
          instance: '/api/audit',
          validationIssues: [],
          fieldErrors: {},
        });
      }

      return securityEvents;
    });

    renderWithProviders(<AuditPage />);

    expect(await screen.findByText('Security audit service is temporarily unavailable.')).toBeTruthy();

    await user.click(screen.getByRole('button', { name: 'Try again' }));

    expect(await screen.findByText('Login succeeded')).toBeTruthy();
    expect(securityAttempts).toBe(2);
  }, 15000);
});
