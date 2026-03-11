import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it } from 'vitest';
import { AuthPage } from '@/features/auth/AuthPage';
import { clearSession } from '@/lib/auth/session';
import { renderWithProviders } from '@/test/render';

describe('AuthPage', () => {
  afterEach(() => {
    clearSession();
    window.sessionStorage.clear();
  });

  it('switches to register mode and exposes the role selector', async () => {
    const user = userEvent.setup();

    renderWithProviders(<AuthPage />);

    const registerTab = screen.getByRole('tab', { name: 'Register' });
    await user.click(registerTab);

    expect(registerTab.getAttribute('aria-selected')).toBe('true');
    expect(screen.getByLabelText('Primary role')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Create account' })).toBeTruthy();
  });
});
