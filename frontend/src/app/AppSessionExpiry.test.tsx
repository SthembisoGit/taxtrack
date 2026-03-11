import { screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SessionExpiredError, apiClient } from '@/lib/api/client';
import { clearSession, saveSession, toAppSession } from '@/lib/auth/session';
import type { AuthResponse } from '@/lib/api/types';
import { renderWithAppRouter } from '@/test/render';

const authResponse: AuthResponse = {
  userId: 'user-1',
  email: 'owner@taxtrack.co.za',
  role: 'Owner',
  accessToken: 'expired-access-token',
  refreshToken: 'expired-refresh-token',
  accessTokenExpiresAtUtc: '2026-03-11T10:00:00Z',
  refreshTokenExpiresAtUtc: '2026-03-11T10:01:00Z',
};

describe('Protected route expiry handling', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it('redirects back to auth when session recovery fails on a protected route', async () => {
    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    vi.spyOn(apiClient, 'listCompanies').mockImplementation(async () => {
      clearSession();
      throw new SessionExpiredError();
    });

    renderWithAppRouter({ initialEntries: ['/dashboard'] });

    expect(await screen.findByRole('button', { name: 'Access workspace' })).toBeTruthy();
    expect(screen.getByText('Spot SARS audit risk before it turns into penalties and rework.')).toBeTruthy();
  }, 15000);
});
