import { useEffect } from 'react';
import { RouterProvider } from 'react-router-dom';
import { AppProviders } from '@/app/providers';
import { router } from '@/app/router';
import { applyThemeTokens } from '@/app/theme';

export function App() {
  useEffect(() => {
    applyThemeTokens();
  }, []);

  return (
    <AppProviders>
      <RouterProvider router={router} />
    </AppProviders>
  );
}
