import type { NormalizedProblem, ValidationIssue } from '@/lib/api/types';

function readValue<T>(source: unknown, keys: string[], fallback: T): T {
  if (!source || typeof source !== 'object') {
    return fallback;
  }

  const record = source as Record<string, unknown>;
  for (const key of keys) {
    const value = record[key];
    if (value !== undefined && value !== null) {
      return value as T;
    }
  }

  return fallback;
}

function normalizeValidationIssue(issue: unknown): ValidationIssue | null {
  if (!issue || typeof issue !== 'object') {
    return null;
  }

  return {
    rowNumber: Number(readValue(issue, ['rowNumber', 'RowNumber'], 0)),
    columnName: String(readValue(issue, ['columnName', 'ColumnName'], 'row')),
    errorCode: String(readValue(issue, ['errorCode', 'ErrorCode'], 'UNKNOWN')),
    message: String(readValue(issue, ['message', 'Message'], 'Validation failed.')),
  };
}

function normalizeFieldErrors(raw: unknown) {
  if (!raw || typeof raw !== 'object' || Array.isArray(raw)) {
    return {};
  }

  const fieldErrors: Record<string, string[]> = {};
  for (const [key, value] of Object.entries(raw)) {
    if (Array.isArray(value)) {
      fieldErrors[key] = value.map((item) => String(item));
    }
  }
  return fieldErrors;
}

export function normalizeProblem(payload: unknown, fallbackStatus = 500): NormalizedProblem {
  const rawErrors = readValue<unknown>(payload, ['errors', 'Errors'], null);
  const validationIssues = Array.isArray(rawErrors)
    ? rawErrors.map(normalizeValidationIssue).filter((issue): issue is ValidationIssue => Boolean(issue))
    : [];

  return {
    type: String(readValue(payload, ['type', 'Type'], 'about:blank')),
    title: String(readValue(payload, ['title', 'Title'], 'Request failed')),
    status: Number(readValue(payload, ['status', 'Status'], fallbackStatus)),
    detail: String(readValue(payload, ['detail', 'Detail'], 'Something went wrong.')),
    instance: String(readValue(payload, ['instance', 'Instance'], '')),
    validationIssues,
    fieldErrors: normalizeFieldErrors(rawErrors),
  };
}
