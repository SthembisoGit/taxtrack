import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { companiesQueryKey, useCompaniesQuery } from '@/features/company/useCompaniesQuery';
import {
  clearSelectedCompany,
  clearSession,
  rememberCompany,
  useAuthSession,
} from '@/lib/auth/session';
import type { CompanyResponse } from '@/lib/api/types';

const emptyCompanies: CompanyResponse[] = [];

const navItems = [
  { to: '/company/setup', label: 'Company' },
  { to: '/upload', label: 'Upload' },
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/report', label: 'Report' },
  { to: '/privacy', label: 'Privacy' },
  { to: '/audit', label: 'Audit' },
];

export function AppShell() {
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const session = useAuthSession();
  const companiesQuery = useCompaniesQuery();
  const companies = companiesQuery.data;

  useEffect(() => {
    if (!session?.selectedCompany || !companiesQuery.isSuccess || companiesQuery.isFetching) {
      return;
    }

    const selectedStillExists = (companies ?? emptyCompanies).some(
      (company) => company.id === session.selectedCompany?.id,
    );
    if (!selectedStillExists) {
      clearSelectedCompany();
      if (location.pathname !== '/company/setup') {
        navigate('/company/setup', { replace: true });
      }
    }
  }, [
    companies,
    companiesQuery.isFetching,
    companiesQuery.isSuccess,
    location.pathname,
    navigate,
    session?.selectedCompany,
  ]);

  function handleWorkspaceChange(companyId: string) {
    const company = (companies ?? emptyCompanies).find((item) => item.id === companyId);
    if (!company) {
      return;
    }

    rememberCompany(company);
    queryClient.removeQueries({ queryKey: ['upload-status'] });
    queryClient.removeQueries({ queryKey: ['analysis-status'] });
    queryClient.removeQueries({ queryKey: ['risk-result'] });
    queryClient.removeQueries({ queryKey: ['report'] });
    queryClient.removeQueries({ queryKey: ['audit-log', 'company'] });
    void queryClient.invalidateQueries({ queryKey: companiesQueryKey });

    if (location.pathname === '/company/setup') {
      navigate('/upload');
    }
  }

  return (
    <div className="app-frame">
      <header className="topbar">
        <div>
          <p className="eyebrow">TaxTrack</p>
          <h1 className="topbar-title">Tax Risk Intelligence</h1>
        </div>

        <nav className="nav-links" aria-label="Primary navigation">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => (isActive ? 'nav-link is-active' : 'nav-link')}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="topbar-meta">
          <div className="topbar-account">
            <p className="meta-label">{session?.email}</p>
            <p className="meta-value">{session?.selectedCompany?.name ?? 'No company selected'}</p>
          </div>
          <label className="topbar-workspace">
            <span className="meta-label">Workspace</span>
            <select
              className="topbar-select"
              disabled={!(companies ?? emptyCompanies).length || companiesQuery.isLoading}
              onChange={(event) => handleWorkspaceChange(event.target.value)}
              value={session?.selectedCompany?.id ?? ''}
            >
              <option value="">
                {companiesQuery.isLoading
                  ? 'Loading workspaces...'
                  : (companies ?? emptyCompanies).length
                    ? 'Select a workspace'
                    : 'No workspaces yet'}
              </option>
              {(companies ?? emptyCompanies).map((company) => (
                <option key={company.id} value={company.id}>
                  {company.name} ({company.registrationNumber})
                </option>
              ))}
            </select>
          </label>
          <div className="topbar-actions">
            <NavLink className="button button-secondary" to="/company/setup">
              Manage workspaces
            </NavLink>
            <button className="button button-secondary" onClick={() => clearSession()} type="button">
              Sign out
            </button>
          </div>
        </div>
      </header>

      <main className="page-shell">
        <Outlet />
      </main>
    </div>
  );
}
