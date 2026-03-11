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

## Notes

- The app currently uses `Database.EnsureCreatedAsync()` for bootstrap simplicity.
- Production migration workflow still needs to be formalized in a later hardening step.
