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

- This bundle is intended for git-repo access handoff.
