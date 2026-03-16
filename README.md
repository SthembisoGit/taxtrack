# TaxTrack

TaxTrack is a SaaS platform for early tax-risk detection for South African companies before SARS audits.

This repository contains approved planning artifacts and an active .NET 8 backend implementation for MVP APIs.

## Core Promise

Upload financial CSV data and receive a Tax Risk Score and risk alerts within seconds.

## Not A Tax Advice Product

TaxTrack provides risk analytics and early warnings. It does not provide legal or tax advice and does not submit taxes on behalf of users.

## Repository Structure

- `/backend` - ASP.NET Core Web API solution (Clean Architecture layers + tests)
- `/frontend` - React + Vite workspace
- `/docs` - product, architecture, contracts, security, backlog, test plans
- `/docs/adr` - architecture decision records
- `/samples` - CSV contract sample files (valid and invalid)
- `/infra` - deployment and environment notes
- `/.github` - pull request template, issue templates, workflow gates

## Recommended Reading Order

1. `docs/0-product-brief.md`
2. `docs/1-scope.md`
3. `docs/3-data-contract.md`
4. `docs/4-risk-rules-catalog.md`
5. `docs/api/openapi-v1.yaml`
6. `docs/12-readiness-checklist.md`

## MVP Technology Targets

- Backend: .NET 8 ASP.NET Core Web API
- Frontend: React + Vite
- Database: PostgreSQL
- ORM: Entity Framework Core
- Auth: JWT access + refresh tokens

## Readiness Rule

Feature coding starts only after all seven gates in `docs/12-readiness-checklist.md` are approved.

## Local Commands

- Backend run: `dotnet run --project backend/src/TaxTrack.Api`
- Backend build: `dotnet build backend/TaxTrack.sln`
- Backend test: `dotnet test backend/TaxTrack.sln`
- Frontend run: `cd frontend && npm run dev`
- Frontend test: `cd frontend && npm run test -- --run`
- Frontend build: `cd frontend && npm run build`
- Validate OpenAPI: `npx -y @apidevtools/swagger-cli validate docs/api/openapi-v1.yaml`

## Environment Templates

- Backend example: `backend/.env.example`
- Frontend example: `frontend/.env.example`

## CORS Configuration

Set `Cors__AllowedOrigins__0` (and additional entries) in `backend/.env` for non-development environments. The API will refuse to start without configured origins when `ASPNETCORE_ENVIRONMENT` is not `Development`.

## Operational Checks

- Liveness: `GET /health/live`
- Readiness: `GET /health/ready`

Use the smoke-check flow in `infra/deployment-smoke-checks.md` after every deploy.
