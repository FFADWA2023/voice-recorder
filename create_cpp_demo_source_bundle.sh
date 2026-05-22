#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DIST_DIR="${ROOT_DIR}/dist"
BUNDLE_NAME="ethical_assistant_cpp_demo_bundle"
BUNDLE_DIR="${DIST_DIR}/${BUNDLE_NAME}"

mkdir -p "${DIST_DIR}"
rm -rf "${BUNDLE_DIR}"
mkdir -p "${BUNDLE_DIR}"

copy_tree() {
  local src="$1"
  local dst="$2"
  mkdir -p "$(dirname "${dst}")"
  rsync -a \
    --exclude '.git' \
    --exclude '.vscode' \
    --exclude 'node_modules' \
    --exclude 'bin' \
    --exclude 'obj' \
    --exclude 'build' \
    --exclude 'build_demo' \
    --exclude 'CMakeFiles' \
    --exclude 'CMakeCache.txt' \
    --exclude '*.o' \
    --exclude '*.a' \
    --exclude '*.so' \
    --exclude '*.zip' \
    "${src}" "${dst}"
}

# 1) Runtime payload only (no C++ source, no build metadata)
copy_tree "${ROOT_DIR}/config" "${BUNDLE_DIR}/"
cp "${ROOT_DIR}/transcribe.sh" "${BUNDLE_DIR}/transcribe.sh"

if [[ ! -f "${ROOT_DIR}/build_demo/ethical_assistant_demo" ]]; then
  echo "Error: compiled binary missing at ${ROOT_DIR}/build_demo/ethical_assistant_demo"
  echo "Build it first, then re-run this bundle script."
  exit 1
fi
cp "${ROOT_DIR}/build_demo/ethical_assistant_demo" "${BUNDLE_DIR}/ethical_assistant_demo"

# 2) Hybrid UI + API source
mkdir -p "${BUNDLE_DIR}/demo_hybrid"
copy_tree "${ROOT_DIR}/demo_hybrid/RecorderBridge.Api" "${BUNDLE_DIR}/demo_hybrid/"
copy_tree "${ROOT_DIR}/demo_hybrid/recorder-ui" "${BUNDLE_DIR}/demo_hybrid/"

# 3) Client helper scripts (binary-only)
mkdir -p "${BUNDLE_DIR}/scripts"
cat > "${BUNDLE_DIR}/scripts/run_backend.sh" << 'EOF'
#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export Recorder__ExecutablePath="${ROOT_DIR}/ethical_assistant_demo"
export Recorder__WorkingDirectory="${ROOT_DIR}"
cd "${ROOT_DIR}/demo_hybrid/RecorderBridge.Api"
dotnet run
EOF

cat > "${BUNDLE_DIR}/scripts/run_frontend.sh" << 'EOF'
#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}/demo_hybrid/recorder-ui"
npm install
npm run dev
EOF

cat > "${BUNDLE_DIR}/scripts/run_demo_stack.sh" << 'EOF'
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
EOF

cat > "${BUNDLE_DIR}/README_CLIENT_BUNDLE.md" << 'EOF'
# Ethical Assistant - Client Repo Bundle

This folder is a binary-only client handoff bundle for running the recorder with the .NET bridge and React UI.

## Included

- Prebuilt recorder executable: `ethical_assistant_demo`
- Backend API source in `demo_hybrid/RecorderBridge.Api/`
- Frontend UI source in `demo_hybrid/recorder-ui/`
- Helper scripts in `scripts/`

## Prerequisites (Linux)

Install runtime dependencies:

```bash
sudo apt update
sudo apt install -y libgtk-3-0 gstreamer1.0-plugins-base curl
```

Install runtimes:

1. .NET SDK 10 (or compatible with project target `net10.0`)
2. Node.js 18+ and npm

## Quick Start (2 terminals)

### 1) Start backend API

```bash
cd scripts
./run_backend.sh
```

Backend runs on:

`http://localhost:5009` (or the configured Kestrel URL)

### 2) Start frontend UI

```bash
cd scripts
./run_frontend.sh
```

Open browser:

`http://localhost:5173`

## One-command run

```bash
cd scripts
./run_demo_stack.sh
```

This starts backend and frontend using the bundled executable.

## Recorder Launch Path

The backend Start button launches this executable:

`ethical_assistant_demo`

This is configured via environment variables in `scripts/run_backend.sh`.

## Troubleshooting

### Start button fails with executable not found

Run:

```bash
ls -l ./ethical_assistant_demo
```

### UI cannot reach backend

Check backend terminal is running, then test:

```bash
curl http://localhost:5009/api/health
```

### Transcribe button says no recording

1. Click Start then Stop once in recorder window.
2. Confirm output dir in config if changed.
3. Ensure `transcribe.sh` is executable:

```bash
chmod +x ./transcribe.sh
```

## Notes

- No C++ source code is included in this bundle.
- This bundle is intended for git-repo access handoff.
EOF

chmod +x "${BUNDLE_DIR}/scripts/"*.sh
chmod +x "${BUNDLE_DIR}/transcribe.sh"

echo "Bundle directory: ${BUNDLE_DIR}"
echo "Bundle mode: repo folder only (no zip)"
