import { screen, fireEvent, waitFor } from '@testing-library/react';
import { Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { AppShell } from '@/app/layout/AppShell';
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

const companies: CompanyResponse[] = [
  {
    id: 'company-1',
    name: 'Acme Holdings',
    registrationNumber: '2018/123456/07',
    industry: 'Technology',
    taxReference: '1234567890',
    ownerUserId: 'user-1',
    createdAtUtc: '2026-03-11T10:00:00Z',
  },
  {
    id: 'company-2',
    name: 'Delta Logistics',
    registrationNumber: '2017/987654/07',
    industry: 'Logistics',
    taxReference: '0987654321',
    ownerUserId: 'user-1',
    createdAtUtc: '2026-03-11T10:00:00Z',
  },
];

describe('AppShell', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it('switches the active workspace from the topbar selector', async () => {
    saveSession(
      toAppSession(authResponse, {
        id: companies[0].id,
        name: companies[0].name,
        registrationNumber: companies[0].registrationNumber,
      }),
    );

    vi.spyOn(apiClient, 'listCompanies').mockResolvedValue(companies);

    renderWithProviders(
      <Routes>
        <Route element={<AppShell />}>
          <Route path="/dashboard" element={<div>Loaded content</div>} />
        </Route>
      </Routes>,
      { route: '/dashboard' },
    );

    expect(await screen.findByText('Loaded content')).toBeTruthy();

    const select = screen.getByRole('combobox');
    await waitFor(() => {
      expect(select.querySelectorAll('option').length).toBeGreaterThan(2);
    });
    fireEvent.change(select, { target: { value: companies[1].id } });

    expect(getSession()?.selectedCompany?.id).toBe(companies[1].id);
    expect(getSession()?.selectedCompany?.name).toBe(companies[1].name);
  }, 15000);
});
