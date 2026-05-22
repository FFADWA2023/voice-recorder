# Hybrid Migration Demo (.NET + React + C++)

This folder gives you a fast client-demo architecture without rewriting the C++ recorder core.

## Architecture

- C++: existing recorder executable remains the recording kernel.
- .NET API: starts/stops recorder process and exposes status.
- React UI: dashboard for client-friendly control and visualization.

## Folders

- `RecorderBridge.Api`: ASP.NET Core API bridge.
- `recorder-ui`: React app (Vite).

## 1) Executable Check

The prebuilt executable should be at:

```text
../build_demo/ethical_assistant_demo
```

This is already configured in `RecorderBridge.Api/appsettings.json`.

## 2) Run .NET API

From the volcacom root directory:

```bash
cd demo_hybrid/RecorderBridge.Api
dotnet run
```

Default dev URL is `http://localhost:5216`.

## 3) Run React UI

From the volcacom root directory (in another terminal):

```bash
cd demo_hybrid/recorder-ui
npm install
npm run dev
```

Open `http://localhost:5173`.

## API Endpoints

- `GET /api/health`
- `GET /api/recorder/status`
- `POST /api/recorder/start`
- `POST /api/recorder/stop`

## Why this is best for urgent presentation

- You keep the most critical C++ logic untouched.
- You show a modern .NET + React product direction immediately.
- You can migrate internals later in small, safe steps.

## Build Instructions (for modifying source)

If you need to rebuild the React UI or .NET backend:

```bash
# React UI build
cd demo_hybrid/recorder-ui && npm run build

# .NET backend build
cd demo_hybrid/RecorderBridge.Api && dotnet build
```
