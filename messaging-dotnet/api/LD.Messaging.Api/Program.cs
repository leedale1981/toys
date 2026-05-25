using System.Diagnostics;
using FastEndpoints;
using LD.Messaging.Api;
using LD.Messaging.Infrastructure.Persistence;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

// ── Bootstrap logger (captures startup errors) ────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) =>
    {
        cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "LD.Messaging.Api")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341");
    });

    // ── OpenTelemetry ─────────────────────────────────────────────────────
    var seqOtlpEndpoint = builder.Configuration["Seq:OtlpEndpoint"] ?? "http://localhost:5341";

    builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(
            serviceName: DiagnosticsConfig.ServiceName,
            serviceVersion: "1.0.0"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(o => o.RecordException = true)
            .AddHttpClientInstrumentation()
            .AddSource(DiagnosticsConfig.SourceName)
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri($"{seqOtlpEndpoint}/ingest/otlp/v1/traces");
                o.Protocol = OtlpExportProtocol.HttpProtobuf;
            }))
        .WithMetrics(metrics => metrics
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri($"{seqOtlpEndpoint}/ingest/otlp/v1/metrics");
                o.Protocol = OtlpExportProtocol.HttpProtobuf;
            }));

    // ── Trace → Log correlation middleware ────────────────────────────────
    // Enriches every Serilog log within a request with its TraceId and SpanId
    // so logs and traces can be correlated in Seq.

    // ── Persistence ───────────────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("StockRecordsDb")
        ?? throw new InvalidOperationException("Connection string 'StockRecordsDb' not found.");
    builder.Services.AddStockRecordsPersistence(connectionString);

    // ── FastEndpoints ─────────────────────────────────────────────────────
    builder.Services.AddFastEndpoints();

    var app = builder.Build();

    // Enrich Serilog logs with the current trace/span IDs on every request
    app.Use(async (context, next) =>
    {
        var activity = Activity.Current;
        using (Serilog.Context.LogContext.PushProperty("TraceId", activity?.TraceId.ToString()))
        using (Serilog.Context.LogContext.PushProperty("SpanId", activity?.SpanId.ToString()))
        {
            await next();
        }
    });

    app.UseSerilogRequestLogging();
    app.MapGet("/health", () => Results.Ok("healthy"));
    app.UseFastEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
