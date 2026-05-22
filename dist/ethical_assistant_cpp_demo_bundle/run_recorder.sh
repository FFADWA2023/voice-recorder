#!/bin/bash

# Ethical Assistant - Production Launcher
# This script sets up the environment and runs the recorder without hardcoded paths

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# Set recordings directory (defaults to ~/recordings)
export ETHICS_RECORDINGS_DIR="${ETHICS_RECORDINGS_DIR:-$HOME/recordings}"

# Set app root to this bundle directory
export ETHICS_ASSISTANT_ROOT="$SCRIPT_DIR"

# Ensure recordings directory exists
mkdir -p "$ETHICS_RECORDINGS_DIR" || {
    echo "ERROR: Cannot create recordings directory at $ETHICS_RECORDINGS_DIR"
    exit 1
}

echo "═══════════════════════════════════════════"
echo "Ethical Assistant Recorder"
echo "═══════════════════════════════════════════"
echo "App Root:       $ETHICS_ASSISTANT_ROOT"
echo "Recordings Dir: $ETHICS_RECORDINGS_DIR"
echo ""

# Check if executable exists
if [ ! -f "$SCRIPT_DIR/ethical_assistant_demo" ]; then
    echo "ERROR: Executable not found at $SCRIPT_DIR/ethical_assistant_demo"
    echo "Make sure you've extracted the bundle completely."
    exit 1
fi

# Run the recorder
"$SCRIPT_DIR/ethical_assistant_demo"
