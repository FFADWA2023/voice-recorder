using RecorderBridge.Api.Models;
using RecorderBridge.Api.Services;

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
