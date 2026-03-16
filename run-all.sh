#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

"$SCRIPT_DIR/backend/run-api.sh" &
API_PID=$!

"$SCRIPT_DIR/frontend/run-frontend.sh"

wait "$API_PID"
