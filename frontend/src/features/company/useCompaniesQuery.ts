import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api/client';
import { useAuthSession } from '@/lib/auth/session';

export const companiesQueryKeyPrefix = ['companies'] as const;

export function companiesQueryKey(userId?: string) {
  return [...companiesQueryKeyPrefix, userId ?? 'anonymous'] as const;
}

export function useCompaniesQuery() {
  const session = useAuthSession();

  return useQuery({
    queryKey: companiesQueryKey(session?.userId),
    enabled: Boolean(session?.userId),
    queryFn: () => apiClient.listCompanies(),
    retry: false,
  });
}
