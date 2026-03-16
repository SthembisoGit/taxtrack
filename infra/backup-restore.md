# Backup And Restore Runbook (MVP)

This runbook applies to Neon PostgreSQL for the MVP. It should be reviewed before every pilot onboarding.

## Backup Schedule

- Daily automated backups (provider-managed).
- Manual backup before every production release or risky migration.
- Retention target: 30 days (or provider maximum for the plan).

## What Must Be Included

- All company financial data.
- Audit logs.
- Risk results and alerts.
- Upload and analysis job history.
- Privacy/data-subject requests.

## Manual Backup (pg_dump)

```bash
pg_dump --format=custom --file=taxtrack.backup --dbname "$DATABASE_URL"
```

Store backups in a secure bucket with encryption at rest and restricted access.

## Restore Procedure (pg_restore)

1. Create a fresh database (new database name).
2. Restore into the new database:

```bash
pg_restore --dbname "$DATABASE_URL" --clean --if-exists taxtrack.backup
```

3. Run `infra/deployment-smoke-checks.md`.
4. Verify audit logs and latest risk results exist.

## Disaster Recovery Checklist

- [ ] Provider backup and restore verified within last 30 days.
- [ ] Manual backup tested within last quarter.
- [ ] RTO target: 4 hours.
- [ ] RPO target: 24 hours.
- [ ] On-call contact and escalation documented.
