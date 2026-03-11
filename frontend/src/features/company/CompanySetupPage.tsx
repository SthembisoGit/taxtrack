import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Panel } from '@/components/ui/Panel';
import { InputField, SelectField } from '@/components/ui/FormField';
import { apiClient, ApiError } from '@/lib/api/client';
import { rememberCompany, useAuthSession } from '@/lib/auth/session';

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
        <h1>Register the company you want TaxTrack to monitor.</h1>
      </div>

      <Panel
        title="Company profile"
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
            <button
              className="button button-secondary"
              onClick={() => navigate('/upload')}
              type="button"
            >
              Continue later
            </button>
          </div>
        </form>
      </Panel>
    </div>
  );
}
