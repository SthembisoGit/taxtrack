# TaxTrack Backend

ASP.NET Core Web API for the TaxTrack MVP.

## Current scope

The backend currently supports:

- registration and login
- refresh token rotation
- company creation and listing
- CSV upload and upload status tracking
- risk analysis and result retrieval
- report retrieval
- audit trail retrieval
- privacy request creation, retrieval, and listing
- health and readiness probes

## Run locally

Run from `backend/`.

```bash
dotnet restore TaxTrack.sln
dotnet run --project src/TaxTrack.Api
```

The API will use:

- PostgreSQL when `ConnectionStrings__PostgreSql` is set
- in-memory EF Core when the connection string is empty

## Verification

```bash
dotnet build TaxTrack.sln
dotnet test TaxTrack.sln
```

## Operational endpoints

- `GET /health/live`
- `GET /health/ready`

`/health/live` confirms the process is running.  
`/health/ready` verifies database connectivity before traffic is sent to the instance.

## Configuration

Copy `.env.example` values into your deployment platform or local secret store.

Primary settings:

- `ConnectionStrings__PostgreSql`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SigningKey`
- `Jwt__AccessTokenMinutes`
- `Jwt__RefreshTokenDays`
- `TaxPolicy__Version`
- `TaxPolicy__EffectiveDate`

## Migrations

Create or update the database schema using EF Core migrations.

Local/dev:

```bash
dotnet ef migrations add InitialCreate --project src/TaxTrack.Infrastructure --startup-project src/TaxTrack.Api --output-dir Migrations
dotnet ef database update --project src/TaxTrack.Infrastructure --startup-project src/TaxTrack.Api
```

Production:

1. Apply migrations in your release pipeline or a controlled maintenance step.
2. The API will refuse to start if pending migrations are detected in production.

## Notes

- The API uses EF Core migrations for relational databases; in-memory mode is used only when no connection string is configured.
