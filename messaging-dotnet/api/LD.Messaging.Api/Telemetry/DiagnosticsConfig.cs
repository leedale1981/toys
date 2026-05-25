using System.Diagnostics;

namespace LD.Messaging.Api;

/// <summary>
/// Central OpenTelemetry diagnostics configuration for the API service.
/// Register <see cref="SourceName"/> with <c>.AddSource(DiagnosticsConfig.SourceName)</c>
/// and use <see cref="ActivitySource"/> to start custom spans around meaningful operations.
/// </summary>
public static class DiagnosticsConfig
{
    public const string ServiceName = "LD.Messaging.Api";
    public const string SourceName = ServiceName;

    /// <summary>
    /// Use this to start custom activity spans:
    /// <code>
    /// using var activity = DiagnosticsConfig.ActivitySource.StartActivity("Describe the operation");
    /// activity?.SetTag("key", value);
    /// </code>
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(SourceName);
}

