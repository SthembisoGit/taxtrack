import { createBrowserRouter } from 'react-router-dom';
import { ProtectedRoute, PublicOnlyRoute, RootRedirect } from '@/app/guards';
import { AuthPage } from '@/features/auth/AuthPage';
import { AuditPage } from '@/features/audit/AuditPage';
import { CompanySetupPage } from '@/features/company/CompanySetupPage';
import { UploadPage } from '@/features/upload/UploadPage';
import { DashboardPage } from '@/features/risk/DashboardPage';
import { ReportPage } from '@/features/report/ReportPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <RootRedirect />,
  },
  {
    element: <PublicOnlyRoute />,
    children: [
      {
        path: '/auth',
        element: <AuthPage />,
      },
    ],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: '/company/setup',
        element: <CompanySetupPage />,
      },
      {
        path: '/upload',
        element: <UploadPage />,
      },
      {
        path: '/dashboard',
        element: <DashboardPage />,
      },
      {
        path: '/report',
        element: <ReportPage />,
      },
      {
        path: '/audit',
        element: <AuditPage />,
      },
    ],
  },
]);
