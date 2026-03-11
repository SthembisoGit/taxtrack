import { describe, expect, it } from 'vitest';
import { normalizeProblem } from '@/lib/utils/problem';

describe('normalizeProblem', () => {
  it('normalizes mixed-case validation payloads into frontend-safe errors', () => {
    const result = normalizeProblem(
      {
        Title: 'Validation failed',
        Status: 422,
        Detail: 'CSV validation failed.',
        Errors: [
          {
            RowNumber: 4,
            ColumnName: 'supplier_vat_number',
            ErrorCode: 'REQUIRED',
            Message: 'Supplier VAT number is required.',
          },
        ],
      },
      500,
    );

    expect(result.title).toBe('Validation failed');
    expect(result.status).toBe(422);
    expect(result.detail).toBe('CSV validation failed.');
    expect(result.validationIssues).toEqual([
      {
        rowNumber: 4,
        columnName: 'supplier_vat_number',
        errorCode: 'REQUIRED',
        message: 'Supplier VAT number is required.',
      },
    ]);
  });
});
