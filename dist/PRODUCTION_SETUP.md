# Ethical Assistant - Production Setup Guide

## Environment Variables

The built bundles use environment variables for all paths. This allows the app to run on any machine without hardcoded paths.

### Required Environment Variables

Set these before running the recorder:

```bash
# Recordings directory (where audio files and chat history are saved)
export ETHICS_RECORDINGS_DIR="/path/to/your/recordings"

# App root directory (where transcribe.sh and other scripts are located)
export ETHICS_ASSISTANT_ROOT="/path/to/bundle/directory"
```

### Optional Environment Variables

```bash
# API keys for optional services
export ETHICS_ASSISTANT_UPLOAD_URL="https://your-api-endpoint.com/upload"
export ETHICS_ASSISTANT_API_KEY="your-api-key-here"
```

## Quick Start - Linux/Mac

### 1. Extract Bundle

```bash
cd /path/to/deployment
unzip ethical_assistant_cpp_demo_bundle.zip
cd ethical_assistant_cpp_demo_bundle
```

### 2. Create Recordings Directory

```bash
mkdir -p ~/recordings
```

### 3. Run with Environment Variables

**Option A: Inline (one-time)**
```bash
ETHICS_RECORDINGS_DIR=~/recordings ETHICS_ASSISTANT_ROOT=$PWD ./ethical_assistant_demo
```

**Option B: Script (recommended for clients)**

Create `run_recorder.sh`:
```bash
#!/bin/bash

# Set directories
export ETHICS_RECORDINGS_DIR="$HOME/recordings"
export ETHICS_ASSISTANT_ROOT="$(cd "$(dirname "$0")" && pwd)"

# Optional: Set API keys if needed
# export ETHICS_ASSISTANT_UPLOAD_URL="https://api.example.com"
# export ETHICS_ASSISTANT_API_KEY="your-key"

# Run the recorder
"$ETHICS_ASSISTANT_ROOT/ethical_assistant_demo"
```

Make it executable:
```bash
chmod +x run_recorder.sh
./run_recorder.sh
```

**Option C: Systemwide (if installed globally)**
```bash
# Add to ~/.bashrc or ~/.zshrc
export ETHICS_RECORDINGS_DIR="$HOME/recordings"
export ETHICS_ASSISTANT_ROOT="/opt/ethical-assistant"

# Then just run:
ethical_assistant_demo
```

## Starting the .NET Backend

The React UI communicates with a .NET API. Start the backend separately:

```bash
cd RecorderBridge.Api
dotnet run
```

Then access the UI at `http://localhost:5173` or the configured port.

## Troubleshooting

### "Recording directory not found"
→ Ensure `ETHICS_RECORDINGS_DIR` is set and the directory exists:
```bash
mkdir -p "$ETHICS_RECORDINGS_DIR"
```

### "Transcription failed to launch"
→ Ensure `ETHICS_ASSISTANT_ROOT` points to the correct bundle directory with `transcribe.sh`.

### "Connection refused" from React UI
→ Ensure the .NET backend is running on the expected port (usually 5000 or 5001).

## File Locations After Setup

```
ETHICS_RECORDINGS_DIR/
├── recording_20260522_120000.wav
├── recording_20260522_120045.wav
├── screenshot_xxx.png
└── conversation_history.json
```

## For Developers

All hardcoded paths have been removed and replaced with environment variables:
- `ETHICS_RECORDINGS_DIR` - fallback: `$HOME/recordings`
- `ETHICS_ASSISTANT_ROOT` - fallback: current directory
