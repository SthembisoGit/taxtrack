import { useSyncExternalStore } from 'react';
import type { AuthResponse, CompanyResponse, UserRole } from '@/lib/api/types';

export interface SelectedCompany {
  id: string;
  name: string;
  registrationNumber: string;
}

export interface AppSession {
  userId: string;
  email: string;
  role: UserRole;
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
  selectedCompany?: SelectedCompany;
}

const STORAGE_KEY = 'taxtrack.session.v1';
const listeners = new Set<() => void>();

let sessionCache = readSessionFromStorage();

function notify() {
  for (const listener of listeners) {
    listener();
  }
}

function readSessionFromStorage(): AppSession | null {
  if (typeof window === 'undefined') {
    return null;
  }

  const raw = window.sessionStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AppSession;
  } catch {
    window.sessionStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

function persistSession(session: AppSession | null) {
  if (typeof window === 'undefined') {
    return;
  }

  if (session) {
    window.sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  } else {
    window.sessionStorage.removeItem(STORAGE_KEY);
  }
}

if (typeof window !== 'undefined') {
  window.addEventListener('storage', (event) => {
    if (event.key === STORAGE_KEY) {
      sessionCache = readSessionFromStorage();
      notify();
    }
  });
}

export function subscribeToSession(listener: () => void) {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

export function getSession() {
  return sessionCache;
}

export function useAuthSession() {
  return useSyncExternalStore(subscribeToSession, getSession, getSession);
}

export function saveSession(session: AppSession) {
  sessionCache = session;
  persistSession(session);
  notify();
}

export function clearSession() {
  sessionCache = null;
  persistSession(null);
  notify();
}

export function mergeSession(patch: Partial<AppSession>) {
  const current = getSession();
  if (!current) {
    return;
  }

  saveSession({ ...current, ...patch });
}

export function toAppSession(auth: AuthResponse, selectedCompany?: SelectedCompany): AppSession {
  return {
    userId: auth.userId,
    email: auth.email,
    role: auth.role,
    accessToken: auth.accessToken,
    refreshToken: auth.refreshToken,
    accessTokenExpiresAt: auth.accessTokenExpiresAt,
    refreshTokenExpiresAt: auth.refreshTokenExpiresAt,
    selectedCompany,
  };
}

export function rememberCompany(company: CompanyResponse) {
  mergeSession({
    selectedCompany: {
      id: company.id,
      name: company.name,
      registrationNumber: company.registrationNumber,
    },
  });
}

export function getDefaultAuthedPath(session: AppSession) {
  return session.selectedCompany ? '/upload' : '/company/setup';
}
