import { screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { UploadPage } from '@/features/upload/UploadPage';
import { ApiError, apiClient } from '@/lib/api/client';
import { clearSession, saveSession, toAppSession } from '@/lib/auth/session';
import type { AuthResponse } from '@/lib/api/types';
import { renderWithProviders } from '@/test/render';

const authResponse: AuthResponse = {
  userId: 'user-1',
  email: 'owner@taxtrack.co.za',
  role: 'Owner',
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  accessTokenExpiresAtUtc: '2026-03-11T10:00:00Z',
  refreshTokenExpiresAtUtc: '2026-03-18T10:00:00Z',
};

describe('UploadPage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearSession();
    window.sessionStorage.clear();
  });

  it('supports drag and drop file selection', async () => {
    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    renderWithProviders(<UploadPage />);

    const file = new File(['header\nvalue'], 'transactions.csv', { type: 'text/csv' });
    const dropzone = screen.getByRole('button', { name: /drag a csv here or browse to select one/i });

    fireEvent.drop(dropzone, {
      dataTransfer: {
        files: [file],
      },
    });

    expect(await screen.findByText('transactions.csv')).toBeTruthy();
  }, 15000);

  it('renders validation issues when the API rejects the upload', async () => {
    const user = userEvent.setup();

    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    vi.spyOn(apiClient, 'uploadFinancial').mockRejectedValue(
      new ApiError({
        type: 'about:blank',
        title: 'Validation failed',
        status: 422,
        detail: 'CSV validation failed.',
        instance: '/api/financial/upload',
        fieldErrors: {},
        validationIssues: [
          {
            rowNumber: 4,
            columnName: 'supplier_vat_number',
            errorCode: 'REQUIRED',
            message: 'Supplier VAT number is required.',
          },
        ],
      }),
    );

    renderWithProviders(<UploadPage />);

    const file = new File(['header\nvalue'], 'transactions.csv', { type: 'text/csv' });
    const fileInput = document.querySelector('input[type="file"]');

    if (!(fileInput instanceof HTMLInputElement)) {
      throw new Error('Expected file input to be present.');
    }

    await user.upload(fileInput, file);

    const form = document.querySelector('form');
    if (!(form instanceof HTMLFormElement)) {
      throw new Error('Expected upload form to be present.');
    }

    fireEvent.submit(form);

    await waitFor(() => {
      expect(apiClient.uploadFinancial).toHaveBeenCalledTimes(1);
      expect(screen.getByText('CSV validation failed.')).toBeTruthy();
    });

    expect(screen.getByText('supplier_vat_number')).toBeTruthy();
    expect(screen.getByText('REQUIRED')).toBeTruthy();
  }, 15000);

  it('shows an actionable message when the API is unreachable', async () => {
    const user = userEvent.setup();

    saveSession(
      toAppSession(authResponse, {
        id: 'company-1',
        name: 'Acme Holdings',
        registrationNumber: '2018/123456/07',
      }),
    );

    vi.spyOn(apiClient, 'uploadFinancial').mockRejectedValue(new TypeError('Failed to fetch'));

    renderWithProviders(<UploadPage />);

    const file = new File(['header\nvalue'], 'transactions.csv', { type: 'text/csv' });
    const fileInput = document.querySelector('input[type="file"]');

    if (!(fileInput instanceof HTMLInputElement)) {
      throw new Error('Expected file input to be present.');
    }

    await user.upload(fileInput, file);

    const form = document.querySelector('form');
    if (!(form instanceof HTMLFormElement)) {
      throw new Error('Expected upload form to be present.');
    }

    fireEvent.submit(form);

    expect(
      await screen.findByText(
        'Could not reach the TaxTrack API. Check that the backend is running and the frontend API URL is correct.',
      ),
    ).toBeTruthy();
  }, 15000);
});
