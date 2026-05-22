#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export Recorder__ExecutablePath="${ROOT_DIR}/ethical_assistant_demo"
export Recorder__WorkingDirectory="${ROOT_DIR}"
cd "${ROOT_DIR}/demo_hybrid/RecorderBridge.Api"
dotnet run
