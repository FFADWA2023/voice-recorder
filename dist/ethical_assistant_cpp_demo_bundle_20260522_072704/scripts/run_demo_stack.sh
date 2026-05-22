#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if [[ ! -f "${ROOT_DIR}/ethical_assistant_demo" ]]; then
  echo "Error: missing executable ${ROOT_DIR}/ethical_assistant_demo"
  exit 1
fi

echo "[1/2] Start backend API in background"
(
  export Recorder__ExecutablePath="${ROOT_DIR}/ethical_assistant_demo"
  export Recorder__WorkingDirectory="${ROOT_DIR}"
  cd "${ROOT_DIR}/demo_hybrid/RecorderBridge.Api"
  dotnet run
) &
API_PID=$!

echo "Backend PID: ${API_PID}"
echo "[2/2] Start frontend (Ctrl+C to stop; backend will be stopped too)"
trap 'kill ${API_PID} >/dev/null 2>&1 || true' EXIT
cd "${ROOT_DIR}/demo_hybrid/recorder-ui"
npm install
npm run dev
