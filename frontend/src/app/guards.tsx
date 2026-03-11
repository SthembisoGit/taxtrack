import { Navigate, Outlet } from 'react-router-dom';
import { AppShell } from '@/app/layout/AppShell';
import { getDefaultAuthedPath, useAuthSession } from '@/lib/auth/session';

export function RootRedirect() {
  const session = useAuthSession();
  return <Navigate replace to={session ? getDefaultAuthedPath(session) : '/auth'} />;
}

export function PublicOnlyRoute() {
  const session = useAuthSession();
  return session ? <Navigate replace to={getDefaultAuthedPath(session)} /> : <Outlet />;
}

export function ProtectedRoute() {
  const session = useAuthSession();
  return session ? <AppShell /> : <Navigate replace to="/auth" />;
}
