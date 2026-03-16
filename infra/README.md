# Infrastructure Notes

Initial deployment targets for MVP:

- Backend API: Railway or Render
- Database: Neon PostgreSQL
- Frontend: Vercel

Scale path:

- Backend: Azure App Service
- Database: Azure Database for PostgreSQL
- Storage: Azure Blob Storage
- Identity: Azure AD B2C
- Monitoring: Azure Application Insights

## Deployment runbooks

- Smoke checks: `infra/deployment-smoke-checks.md`
- Backup and restore: `infra/backup-restore.md`
