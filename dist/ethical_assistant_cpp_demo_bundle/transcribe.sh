#!/bin/bash
# Transcribe WAV recording to text script

set -e

if [ $# -lt 1 ]; then
    echo "Usage: $0 <wav_file> [output_file]"
    echo ""
    echo "Example:"
    echo "  $0 $HOME/recordings/recording_20251111_105114.wav"
    echo "  $0 $HOME/recordings/recording_20251111_105114.wav script.txt"
    
    exit 1
fi

WAV_FILE="$1"
OUTPUT_FILE="${2:-${WAV_FILE%.wav}_script.txt}"

if [ ! -f "$WAV_FILE" ]; then
    echo "Error: File not found: $WAV_FILE"
    exit 1
fi

echo "🎙️  Transcribing audio to script..."
echo "Input:  $WAV_FILE"
echo "Output: $OUTPUT_FILE"
echo ""

# Use Whisper to transcribe
whisper "$WAV_FILE" --output_format txt --output_dir /tmp --model base --language en

# Move transcript to output location
TMP_SCRIPT="/tmp/$(basename "${WAV_FILE%.wav}").txt"
if [ -f "$TMP_SCRIPT" ]; then
    mv "$TMP_SCRIPT" "$OUTPUT_FILE"
    echo ""
    echo "✅ Transcription complete!"
    echo ""
    echo "📝 Script saved to: $OUTPUT_FILE"
    echo ""
    echo "Content:"
    echo "---"
    cat "$OUTPUT_FILE"
    echo "---"
else
    echo "Error: Transcription failed"
    exit 1
fi
