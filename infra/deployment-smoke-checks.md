# Deployment Smoke Checks

Run this checklist after every deploy to Railway, Render, Vercel, or any later production target.

## 1. API health

```bash
curl -i https://your-api-host/health/live
curl -i https://your-api-host/health/ready
```

Expected:

- `/health/live` returns `200`
- `/health/ready` returns `200`
- response body includes `"status": "Healthy"`

## 2. Auth path

Verify:

- register a test user
- log in successfully
- refresh token rotation works once
- replaying the old refresh token returns `401`

## 3. Core workflow

Verify a complete happy path:

1. create a company
2. upload a valid transactions CSV
3. run analysis
4. open dashboard
5. open report
6. confirm dashboard/report risk score alignment

## 4. Compliance workflow

Verify:

- audit page loads workspace and personal events
- privacy page lists existing requests
- a new export or deletion request can be submitted

## 5. Operational config

Confirm these are set correctly in the deployment platform:

- `ConnectionStrings__PostgreSql`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SigningKey`
- `VITE_API_BASE_URL`

## 6. Failure checks

Verify:

- protected routes redirect to `/auth` when refresh recovery fails
- retry buttons recover on dashboard, report, audit, and privacy pages after a transient API failure

## 7. Release sign-off

Record:

- deployed commit SHA
- deploy timestamp (UTC)
- environment name
- person who ran the smoke check
- pass/fail result
