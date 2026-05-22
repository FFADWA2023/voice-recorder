using RecorderBridge.Api.Models;
using RecorderBridge.Api.Services;

static void LoadEnvFile(string envPath)
{
    if (!File.Exists(envPath))
    {
        return;
    }

    foreach (var raw in File.ReadAllLines(envPath))
    {
        var line = raw.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
        {
            continue;
        }

        var idx = line.IndexOf('=');
        if (idx <= 0)
        {
            continue;
        }

        var key = line.Substring(0, idx).Trim();
        var value = line.Substring(idx + 1).Trim();

        if (value.Length >= 2 &&
            ((value.StartsWith('"') && value.EndsWith('"')) ||
             (value.StartsWith('\'') && value.EndsWith('\''))))
        {
            value = value.Substring(1, value.Length - 2);
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            continue;
        }

        // Keep an already-exported shell variable as higher priority.
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

var envCandidates = new[]
{
    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".env")),
    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env")),
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env")),
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env"))
};

foreach (var candidate in envCandidates)
{
    LoadEnvFile(candidate);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RecorderOptions>(builder.Configuration.GetSection("Recorder"));
builder.Services.AddSingleton<IRecorderBridgeService, RecorderBridgeService>();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("frontend");

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "RecorderBridge.Api",
    utc = DateTimeOffset.UtcNow
}));

app.MapGet("/api/recorder/status", (IRecorderBridgeService recorderBridge) =>
    Results.Ok(recorderBridge.GetStatus()));

app.MapPost("/api/recorder/start", (IRecorderBridgeService recorderBridge) =>
{
    var result = recorderBridge.Start();
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/recorder/stop", (IRecorderBridgeService recorderBridge) =>
{
    var result = recorderBridge.Stop();
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.Run();
