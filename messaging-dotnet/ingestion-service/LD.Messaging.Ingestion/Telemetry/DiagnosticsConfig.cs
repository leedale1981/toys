using System.Diagnostics;

namespace LD.Messaging.Ingestion;

/// <summary>
/// Central OpenTelemetry diagnostics configuration for the Ingestion service.
/// </summary>
public static class DiagnosticsConfig
{
    public const string ServiceName = "LD.Messaging.Ingestion";
    public const string SourceName = ServiceName;

    /// <summary>
    /// Use this to start custom activity spans around meaningful business operations:
    /// <code>
    /// using var activity = DiagnosticsConfig.ActivitySource.StartActivity("Process FTSE500 batch");
    /// activity?.SetTag("exchange", "FTSE500");
    /// </code>
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(SourceName);
}

