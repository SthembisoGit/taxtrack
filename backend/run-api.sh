#!/usr/bin/env bash
set -euo pipefail

if [[ -f "backend/.env" ]]; then
  while IFS= read -r line; do
    [[ -z "$line" || "$line" =~ ^# ]] && continue
    key="${line%%=*}"
    value="${line#*=}"
    value="${value%\"}"
    value="${value#\"}"
    export "$key=$value"
  done < "backend/.env"
fi

dotnet run --project backend/src/TaxTrack.Api
