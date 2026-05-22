using System.Diagnostics;
using Microsoft.Extensions.Options;
using RecorderBridge.Api.Models;

namespace RecorderBridge.Api.Services;

public sealed record RecorderStatus(
    bool IsRunning,
    int? ProcessId,
    DateTimeOffset? StartedAtUtc,
    string ExecutablePath,
    string? LastError,
    int? LastProcessId,
    DateTimeOffset? LastStartedAtUtc,
    int? LastExitCode,
    DateTimeOffset? LastExitedAtUtc
);

public sealed record RecorderActionResult(bool Success, string Message, RecorderStatus Status);

public interface IRecorderBridgeService
{
    RecorderStatus GetStatus();
    RecorderActionResult Start();
    RecorderActionResult Stop();
}

public sealed class RecorderBridgeService : IRecorderBridgeService, IDisposable
{
    private readonly object _sync = new();
    private readonly ILogger<RecorderBridgeService> _logger;
    private readonly RecorderOptions _options;

    private Process? _process;
    private DateTimeOffset? _startedAtUtc;
    private string? _lastError;
    private int? _lastProcessId;
    private DateTimeOffset? _lastStartedAtUtc;
    private int? _lastExitCode;
    private DateTimeOffset? _lastExitedAtUtc;
    private string? _lastStderrLine;

    public RecorderBridgeService(
        IOptions<RecorderOptions> options,
        ILogger<RecorderBridgeService> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    public RecorderStatus GetStatus()
    {
        lock (_sync)
        {
            if (_process is { HasExited: true })
            {
                _lastExitCode = _process.ExitCode;
                _lastExitedAtUtc = DateTimeOffset.UtcNow;
                _process.Dispose();
                _process = null;
                _startedAtUtc = null;
            }

            if (!string.IsNullOrWhiteSpace(_lastStderrLine))
            {
                _lastError = _lastStderrLine;
            }
            else if (_lastExitCode is int code && code != 0 && string.IsNullOrWhiteSpace(_lastError))
            {
                _lastError = $"Recorder exited with code {code}.";
            }

            return new RecorderStatus(
                IsRunning: _process is { HasExited: false },
                ProcessId: _process is { HasExited: false } ? _process.Id : null,
                StartedAtUtc: _startedAtUtc,
                ExecutablePath: _options.ExecutablePath,
                LastError: _lastError,
                LastProcessId: _lastProcessId,
                LastStartedAtUtc: _lastStartedAtUtc,
                LastExitCode: _lastExitCode,
                LastExitedAtUtc: _lastExitedAtUtc
            );
        }
    }

    public RecorderActionResult Start()
    {
        lock (_sync)
        {
            if (_process is { HasExited: false })
            {
                return new RecorderActionResult(false, "Recorder is already running.", GetStatus());
            }

            if (!File.Exists(_options.ExecutablePath))
            {
                _lastError = $"Recorder executable not found: {_options.ExecutablePath}";
                return new RecorderActionResult(false, _lastError, GetStatus());
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = _options.ExecutablePath,
                Arguments = _options.StartArguments,
                WorkingDirectory = string.IsNullOrWhiteSpace(_options.WorkingDirectory)
                    ? Path.GetDirectoryName(_options.ExecutablePath) ?? "."
                    : _options.WorkingDirectory,
                UseShellExecute = _options.UseShellExecute,
                RedirectStandardOutput = !_options.UseShellExecute,
                RedirectStandardError = !_options.UseShellExecute,
                CreateNoWindow = !_options.UseShellExecute
            };

            foreach (var (key, value) in _options.EnvironmentVariables)
            {
                startInfo.Environment[key] = value;
            }

            try
            {
                var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
                process.ErrorDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrWhiteSpace(args.Data))
                    {
                        lock (_sync)
                        {
                            _lastStderrLine = args.Data;
                        }
                    }
                };
                process.Exited += (_, _) =>
                {
                    lock (_sync)
                    {
                        _lastExitCode = process.ExitCode;
                        _lastExitedAtUtc = DateTimeOffset.UtcNow;

                        if (process.ExitCode != 0)
                        {
                            _lastError = string.IsNullOrWhiteSpace(_lastStderrLine)
                                ? $"Recorder exited with code {process.ExitCode}."
                                : _lastStderrLine;
                            _logger.LogWarning("Recorder process exited with code {ExitCode}", process.ExitCode);
                        }
                    }
                };

                if (!process.Start())
                {
                    _lastError = "Recorder process failed to start.";
                    process.Dispose();
                    return new RecorderActionResult(false, _lastError, GetStatus());
                }

                _process = process;
                _startedAtUtc = DateTimeOffset.UtcNow;
                _lastProcessId = process.Id;
                _lastStartedAtUtc = _startedAtUtc;
                _lastExitCode = null;
                _lastExitedAtUtc = null;
                _lastStderrLine = null;
                _lastError = null;

                if (!startInfo.UseShellExecute)
                {
                    process.BeginErrorReadLine();
                }

                _logger.LogInformation("Recorder started with PID {Pid}", process.Id);
                return new RecorderActionResult(true, "Recorder started.", GetStatus());
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                _logger.LogError(ex, "Failed to start recorder process.");
                return new RecorderActionResult(false, "Failed to start recorder process.", GetStatus());
            }
        }
    }

    public RecorderActionResult Stop()
    {
        lock (_sync)
        {
            if (_process is null || _process.HasExited)
            {
                _process?.Dispose();
                _process = null;
                _startedAtUtc = null;
                return new RecorderActionResult(false, "Recorder is not running.", GetStatus());
            }

            try
            {
                var process = _process;
                _logger.LogInformation("Stopping recorder with PID {Pid}", process.Id);

                if (!process.CloseMainWindow())
                {
                    process.Kill(true);
                }
                else if (!process.WaitForExit(3000))
                {
                    process.Kill(true);
                }

                process.Dispose();
                _process = null;
                _startedAtUtc = null;

                return new RecorderActionResult(true, "Recorder stopped.", GetStatus());
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                _logger.LogError(ex, "Failed to stop recorder process.");
                return new RecorderActionResult(false, "Failed to stop recorder process.", GetStatus());
            }
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_process is null)
            {
                return;
            }

            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(true);
                }
            }
            catch
            {
                // Ignore dispose-time failures.
            }
            finally
            {
                _process.Dispose();
                _process = null;
                _startedAtUtc = null;
            }
        }
    }
}