# TaxTrack Frontend

React + TypeScript frontend for the TaxTrack MVP.

## Scope

This frontend slice covers the first end-to-end user flow:

- register and log in
- create a company profile
- upload CSV datasets
- trigger tax risk analysis
- review the dashboard
- open report download links returned by the API
- submit POPIA-style export/deletion requests and track request status

## Stack

- React 19
- TypeScript
- Vite
- React Router
- TanStack Query
- Vitest + Testing Library

## Commands

Run from `frontend/`.

```bash
npm install
npm run dev
npm run build
npm run test
npm run lint
```

## Environment

Optional frontend variables:

- `VITE_API_BASE_URL`: absolute API base URL for non-proxied environments
- `VITE_API_PROXY_TARGET`: local API target used by the Vite dev proxy

If `VITE_API_BASE_URL` is not set, the app calls relative `/api/*` routes.

## Structure

- `src/app`: app shell, routing, providers, theme loading
- `src/components/ui`: reusable UI primitives
- `src/features`: feature-focused screens
- `src/lib/api`: API client and DTOs
- `src/lib/auth`: session state and route context
- `src/lib/utils`: shared helpers

## Design rules

- Use semantic theme tokens, not hardcoded colors in components.
- Keep business logic in hooks and utilities, not presentational UI.
- Consume API risk levels and severities as semantic values, never as color values.
- Preserve the calm enterprise visual system defined in `docs/13-brand-system.md`.
