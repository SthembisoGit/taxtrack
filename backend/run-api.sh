#!/usr/bin/env bash
set -euo pipefail

if [[ -f "backend/.env" ]]; then
  while IFS= read -r line; do
    [[ -z "$line" || "$line" =~ ^# ]] && continue
    key="${line%%=*}"
    value="${line#*=}"
    key="$(echo "$key" | xargs)"
    value="$(echo "$value" | xargs)"
    value="${value%\"}"
    value="${value#\"}"
    export "$key=$value"
  done < "backend/.env"
fi

if [[ -z "${ASPNETCORE_URLS:-}" ]]; then
  export ASPNETCORE_URLS="http://localhost:8080"
fi

dotnet run --project backend/src/TaxTrack.Api
