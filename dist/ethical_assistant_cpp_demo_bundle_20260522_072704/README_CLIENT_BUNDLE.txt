ETHICAL ASSISTANT - BINARY-ONLY CLIENT BUNDLE

This package contains:
- Prebuilt recorder executable (ethical_assistant_demo)
- .NET RecorderBridge API source
- React UI source

Quick start on client machine:
1) Install dependencies:
   - CMake + GCC/G++
   - GTK3 dev + GStreamer dev + curl dev
   - .NET SDK (net10.0)
   - Node.js + npm

2) Start backend API:
   ./scripts/run_backend.sh

3) Start frontend UI in another terminal:
   ./scripts/run_frontend.sh

4) Open browser:
   http://localhost:5173

One-command run (optional):
   ./scripts/run_demo_stack.sh

Notes:
- Backend Start button launches: ethical_assistant_demo
- No .NET publish step is required.
- No C++ source code is included in this bundle.
