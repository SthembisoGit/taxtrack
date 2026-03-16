#!/usr/bin/env bash
set -euo pipefail

if [[ -f "frontend/.env" ]]; then
  while IFS= read -r line; do
    [[ -z "$line" || "$line" =~ ^# ]] && continue
    key="${line%%=*}"
    value="${line#*=}"
    key="$(echo "$key" | xargs)"
    value="$(echo "$value" | xargs)"
    value="${value%\"}"
    value="${value#\"}"
    export "$key=$value"
  done < "frontend/.env"
fi

cd frontend
if [[ ! -d "node_modules" ]]; then
  npm install
fi
npm run dev
