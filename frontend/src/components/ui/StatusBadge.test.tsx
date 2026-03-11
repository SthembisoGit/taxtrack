import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { StatusBadge } from '@/components/ui/StatusBadge';

describe('StatusBadge', () => {
  it('maps low risk to the success token class', () => {
    render(<StatusBadge riskLevel="Low" />);

    expect(screen.getByText('Low').className).toContain('badge-success');
  });

  it('maps medium risk to the warning token class', () => {
    render(<StatusBadge riskLevel="Medium" />);

    expect(screen.getByText('Medium').className).toContain('badge-warning');
  });

  it('maps high risk to the danger token class', () => {
    render(<StatusBadge riskLevel="High" />);

    expect(screen.getByText('High').className).toContain('badge-danger');
  });

  it('maps critical alert severity to the danger token class', () => {
    render(<StatusBadge severity="Critical" />);

    expect(screen.getByText('Critical').className).toContain('badge-danger');
  });
});
