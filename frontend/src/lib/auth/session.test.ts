import { beforeEach, describe, expect, it } from 'vitest';
import {
  clearSession,
  getSession,
  rememberCompany,
  saveSession,
  toAppSession,
} from '@/lib/auth/session';
import type { AuthResponse, CompanyResponse } from '@/lib/api/types';

const authResponse: AuthResponse = {
  userId: 'user-1',
  email: 'owner@taxtrack.co.za',
  role: 'Owner',
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  accessTokenExpiresAt: '2026-03-11T10:00:00Z',
  refreshTokenExpiresAt: '2026-03-18T10:00:00Z',
};

const company: CompanyResponse = {
  id: 'company-1',
  name: 'Acme Holdings',
  registrationNumber: '2018/123456/07',
  industry: 'Technology',
  taxReference: '1234567890',
  ownerUserId: 'user-1',
  createdAt: '2026-03-11T10:00:00Z',
};

describe('session storage', () => {
  beforeEach(() => {
    window.sessionStorage.clear();
    clearSession();
  });

  it('persists and clears the authenticated session', () => {
    const session = toAppSession(authResponse);

    saveSession(session);

    expect(getSession()).toEqual(session);
    expect(window.sessionStorage.getItem('taxtrack.session.v1')).not.toBeNull();

    clearSession();

    expect(getSession()).toBeNull();
    expect(window.sessionStorage.getItem('taxtrack.session.v1')).toBeNull();
  });

  it('updates the selected company in the existing session', () => {
    saveSession(toAppSession(authResponse));

    rememberCompany(company);

    expect(getSession()?.selectedCompany).toEqual({
      id: company.id,
      name: company.name,
      registrationNumber: company.registrationNumber,
    });
  });
});
