import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { CompanySetupPage } from '@/features/company/CompanySetupPage';
import { apiClient } from '@/lib/api/client';
import { clearSession, getSession, saveSession, toAppSession } from '@/lib/auth/session';
import type { AuthResponse, CompanyResponse } from '@/lib/api/types';
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

const company: CompanyResponse = {
  id: 'company-1',
  name: 'Acme Holdings',
  registrationNumber: '2018/123456/07',
  industry: 'Technology',
  taxReference: '1234567890',
  ownerUserId: 'user-1',
  createdAtUtc: '2026-03-11T10:00:00Z',
};

describe('CompanySetupPage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it('shows existing companies and stores the selected workspace', async () => {
    const user = userEvent.setup();

    saveSession(toAppSession(authResponse));
    vi.spyOn(apiClient, 'listCompanies').mockResolvedValue([company]);

    renderWithProviders(<CompanySetupPage />);

    expect(await screen.findByText('Acme Holdings')).toBeTruthy();
    await user.click(screen.getByRole('button', { name: 'Use workspace' }));

    expect(getSession()?.selectedCompany?.id).toBe(company.id);
    expect(getSession()?.selectedCompany?.name).toBe(company.name);
  });
});
