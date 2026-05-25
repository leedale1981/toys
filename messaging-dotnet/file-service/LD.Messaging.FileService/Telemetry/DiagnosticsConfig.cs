using System.Diagnostics;

namespace LD.Messaging.FileService;

/// <summary>
/// Central OpenTelemetry diagnostics configuration for the File Service.
/// </summary>
public static class DiagnosticsConfig
{
    public const string ServiceName = "LD.Messaging.FileService";
    public const string SourceName = ServiceName;

    /// <summary>
    /// Use this to start custom activity spans around meaningful business operations:
    /// <code>
    /// using var activity = DiagnosticsConfig.ActivitySource.StartActivity("Process stock file");
    /// activity?.SetTag("file.name", fileName);
    /// </code>
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(SourceName);
}

