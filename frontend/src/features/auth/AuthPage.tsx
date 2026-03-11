import { startTransition, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Panel } from '@/components/ui/Panel';
import { InputField, SelectField } from '@/components/ui/FormField';
import { apiClient, ApiError, SessionExpiredError } from '@/lib/api/client';
import { getDefaultAuthedPath, saveSession, toAppSession } from '@/lib/auth/session';

const roles = ['Owner', 'Accountant', 'FinanceManager'] as const;

export function AuthPage() {
  const navigate = useNavigate();
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState<(typeof roles)[number]>('Owner');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError('');

    try {
      if (mode === 'register') {
        await apiClient.register({ email, password, role });
      }

      const auth = await apiClient.login({ email, password });
      const session = toAppSession(auth);
      saveSession(session);
      navigate(getDefaultAuthedPath(session), { replace: true });
    } catch (caught) {
      if (caught instanceof SessionExpiredError) {
        setError(caught.message);
      } else if (caught instanceof ApiError) {
        setError(caught.problem.detail);
      } else {
        setError('We could not complete that request. Try again.');
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-layout">
      <section className="auth-hero">
        <p className="eyebrow">Rule-based tax intelligence</p>
        <h1>Spot SARS audit risk before it turns into penalties and rework.</h1>
        <p className="hero-copy">
          TaxTrack helps owners, accountants, and finance managers upload structured financial
          data, run deterministic risk analysis, and act on clear compliance warnings.
        </p>
        <div className="hero-points">
          <div>
            <strong>Early warning</strong>
            <span>Detect VAT, payroll, and expense anomalies before submission pressure hits.</span>
          </div>
          <div>
            <strong>Explainable output</strong>
            <span>Every score is backed by alerts, evidence completeness, and visible rules.</span>
          </div>
        </div>
      </section>

      <Panel className="auth-panel">
        <div className="tab-strip" role="tablist" aria-label="Authentication mode">
          <button
            aria-controls="auth-panel-login"
            aria-selected={mode === 'login'}
            className={mode === 'login' ? 'tab is-active' : 'tab'}
            id="auth-tab-login"
            onClick={() => startTransition(() => setMode('login'))}
            role="tab"
            type="button"
          >
            Login
          </button>
          <button
            aria-controls="auth-panel-register"
            aria-selected={mode === 'register'}
            className={mode === 'register' ? 'tab is-active' : 'tab'}
            id="auth-tab-register"
            onClick={() => startTransition(() => setMode('register'))}
            role="tab"
            type="button"
          >
            Register
          </button>
        </div>

        <div
          aria-labelledby={mode === 'login' ? 'auth-tab-login' : 'auth-tab-register'}
          id={mode === 'login' ? 'auth-panel-login' : 'auth-panel-register'}
          role="tabpanel"
        >
          <form className="stack" onSubmit={handleSubmit}>
            {error ? <div className="banner banner-error">{error}</div> : null}

            <InputField
              autoComplete="email"
              label="Email"
              onChange={(event) => setEmail(event.target.value)}
              placeholder="name@company.co.za"
              required
              type="email"
              value={email}
            />
            <InputField
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              helperText={mode === 'register' ? 'Use at least 12 characters.' : undefined}
              label="Password"
              minLength={12}
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />

            {mode === 'register' ? (
              <SelectField
                label="Primary role"
                onChange={(event) => setRole(event.target.value as (typeof roles)[number])}
                value={role}
              >
                {roles.map((item) => (
                  <option key={item} value={item}>
                    {item}
                  </option>
                ))}
              </SelectField>
            ) : null}

            <button
              aria-busy={loading}
              className="button button-primary"
              disabled={loading}
              type="submit"
            >
              {loading ? 'Working...' : mode === 'login' ? 'Access workspace' : 'Create account'}
            </button>
          </form>
        </div>
      </Panel>
    </div>
  );
}
