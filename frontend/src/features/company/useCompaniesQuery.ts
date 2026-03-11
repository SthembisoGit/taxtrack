import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api/client';

export const companiesQueryKey = ['companies'] as const;

export function useCompaniesQuery() {
  return useQuery({
    queryKey: companiesQueryKey,
    queryFn: () => apiClient.listCompanies(),
    retry: false,
  });
}
