namespace RecorderBridge.Api.Models;

public sealed class RecorderOptions
{
    public string ExecutablePath { get; set; } = "../../build_demo/ethical_assistant_demo";
    public string WorkingDirectory { get; set; } = "../..";
    public string StartArguments { get; set; } = string.Empty;
    public bool UseShellExecute { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; } = [];
}