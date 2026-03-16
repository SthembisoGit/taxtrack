#!/usr/bin/env bash
set -euo pipefail

if [[ -f "frontend/.env" ]]; then
  while IFS= read -r line; do
    [[ -z "$line" || "$line" =~ ^# ]] && continue
    key="${line%%=*}"
    value="${line#*=}"
    value="${value%\"}"
    value="${value#\"}"
    export "$key=$value"
  done < "frontend/.env"
fi

cd frontend
npm install
npm run dev
