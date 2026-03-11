export function generateIdempotencyKey() {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }

  return `taxtrack-${Date.now()}-${Math.random().toString(16).slice(2)}`;
}
