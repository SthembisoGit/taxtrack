import { useState, type FormEvent } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Panel } from '@/components/ui/Panel';
import { InputField, SelectField } from '@/components/ui/FormField';
import { apiClient, ApiError } from '@/lib/api/client';
import { rememberCompany, useAuthSession } from '@/lib/auth/session';
import { companiesQueryKey, useCompaniesQuery } from '@/features/company/useCompaniesQuery';

const industries = [
  'Manufacturing',
  'Retail',
  'Logistics',
  'ProfessionalServices',
  'Technology',
  'Construction',
  'Hospitality',
  'Other',
];

export function CompanySetupPage() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const session = useAuthSession();
  const [name, setName] = useState(session?.selectedCompany?.name ?? '');
  const [registrationNumber, setRegistrationNumber] = useState(
    session?.selectedCompany?.registrationNumber ?? '',
  );
  const [industry, setIndustry] = useState('Technology');
  const [taxReference, setTaxReference] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const companiesQuery = useCompaniesQuery();
  const companies = companiesQuery.data ?? [];

  function handleSelectCompany(companyId: string) {
    const company = companies.find((item) => item.id === companyId);
    if (!company) {
      return;
    }

    rememberCompany(company);
    navigate('/upload');
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError('');

    try {
      const company = await apiClient.createCompany({
        name,
        registrationNumber,
        industry,
        taxReference,
      });

      void queryClient.invalidateQueries({ queryKey: companiesQueryKey });
      rememberCompany(company);
      navigate('/upload');
    } catch (caught) {
      if (caught instanceof ApiError) {
        setError(caught.problem.detail);
      } else {
        setError('We could not save the company profile. Try again.');
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="stack gap-lg">
      <div className="page-heading">
        <p className="eyebrow">Workspace setup</p>
        <h1>Choose an existing workspace or register a new company.</h1>
      </div>

      {companiesQuery.isLoading ? (
        <Panel title="Loading workspaces" subtitle="Retrieving the companies linked to your account.">
          <div className="skeleton-grid">
            <div className="skeleton-block" />
            <div className="skeleton-block" />
          </div>
        </Panel>
      ) : null}

      {companiesQuery.error instanceof ApiError ? (
        <Panel title="Workspace retrieval failed" subtitle="We could not load your existing companies.">
          <div className="banner banner-error">{companiesQuery.error.problem.detail}</div>
        </Panel>
      ) : null}

      {companies.length ? (
        <Panel
          title="Existing workspaces"
          subtitle="Select a company you already manage, or create another one below."
        >
          <div className="company-grid">
            {companies.map((company) => {
              const isSelected = session?.selectedCompany?.id === company.id;

              return (
                <div
                  key={company.id}
                  className={isSelected ? 'company-card is-selected' : 'company-card'}
                >
                  <div className="company-card-copy">
                    <strong>{company.name}</strong>
                    <span>{company.registrationNumber}</span>
                    <span>{company.industry}</span>
                  </div>
                  <button
                    className={isSelected ? 'button button-primary' : 'button button-secondary'}
                    onClick={() => handleSelectCompany(company.id)}
                    type="button"
                  >
                    {isSelected ? 'Current workspace' : 'Use workspace'}
                  </button>
                </div>
              );
            })}
          </div>
        </Panel>
      ) : null}

      <Panel
        title={companies.length ? 'Create another company' : 'Company profile'}
        subtitle="This company becomes the working context for upload, analysis, and reporting."
      >
        <form className="grid-form" onSubmit={handleSubmit}>
          {error ? <div className="banner banner-error full-span">{error}</div> : null}

          <InputField
            label="Company name"
            onChange={(event) => setName(event.target.value)}
            required
            value={name}
          />
          <InputField
            label="Registration number"
            onChange={(event) => setRegistrationNumber(event.target.value)}
            required
            value={registrationNumber}
          />
          <SelectField
            label="Industry"
            onChange={(event) => setIndustry(event.target.value)}
            value={industry}
          >
            {industries.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </SelectField>
          <InputField
            label="Tax reference"
            minLength={10}
            onChange={(event) => setTaxReference(event.target.value)}
            required
            value={taxReference}
          />

          <div className="button-row full-span">
            <button className="button button-primary" disabled={loading} type="submit">
              {loading ? 'Saving...' : 'Save company'}
            </button>
            {session?.selectedCompany ? (
              <button
                className="button button-secondary"
                onClick={() => navigate('/upload')}
                type="button"
              >
                Open selected workspace
              </button>
            ) : null}
          </div>
        </form>
      </Panel>
    </div>
  );
}
