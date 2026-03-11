import { NavLink, Outlet } from 'react-router-dom';
import { clearSession, useAuthSession } from '@/lib/auth/session';

const navItems = [
  { to: '/company/setup', label: 'Company' },
  { to: '/upload', label: 'Upload' },
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/report', label: 'Report' },
];

export function AppShell() {
  const session = useAuthSession();

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
          <div>
            <p className="meta-label">{session?.email}</p>
            <p className="meta-value">
              {session?.selectedCompany?.name ?? 'No company selected'}
            </p>
          </div>
          <button className="button button-secondary" onClick={() => clearSession()} type="button">
            Sign out
          </button>
        </div>
      </header>

      <main className="page-shell">
        <Outlet />
      </main>
    </div>
  );
}
