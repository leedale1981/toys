using Confluent.Kafka;
using LD.Messaging.Domain.Messages;
using LD.Messaging.Ingestion;
using LD.Messaging.Ingestion.Consumers;
using LD.Messaging.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────
    builder.Services.AddSerilog((services, cfg) =>
    {
        cfg
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "LD.Messaging.Ingestion")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341");
    });

    // ── OpenTelemetry ─────────────────────────────────────────────────────
    var seqOtlpEndpoint = builder.Configuration["Seq:OtlpEndpoint"] ?? "http://localhost:5341";

    builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(
            serviceName: DiagnosticsConfig.ServiceName,
            serviceVersion: "1.0.0"))
        .WithTracing(tracing => tracing
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

    // ── Persistence ───────────────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("StockRecordsDb")
        ?? throw new InvalidOperationException("Connection string 'StockRecordsDb' not found.");
    builder.Services.AddStockRecordsPersistence(connectionString);

    // ── MassTransit + Kafka ───────────────────────────────────────────────
    builder.Services.AddMassTransit(x =>
    {
        x.UsingInMemory();

        x.AddRider(rider =>
        {
            rider.AddConsumer<Ftse500Consumer>();
            rider.AddConsumer<NyseConsumer>();
            rider.AddConsumer<NasdaqConsumer>();

            rider.UsingKafka((ctx, k) =>
            {
                k.Host(builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092");

                k.TopicEndpoint<Ftse500Data>("FTSE500", "ingestion-consumer-group", e =>
                {
                    e.AutoOffsetReset = AutoOffsetReset.Earliest;
                    e.ConfigureConsumer<Ftse500Consumer>(ctx);
                });

                k.TopicEndpoint<NyseData>("NYSE", "ingestion-consumer-group", e =>
                {
                    e.AutoOffsetReset = AutoOffsetReset.Earliest;
                    e.ConfigureConsumer<NyseConsumer>(ctx);
                });

                k.TopicEndpoint<NasdaqData>("NASDAQ", "ingestion-consumer-group", e =>
                {
                    e.AutoOffsetReset = AutoOffsetReset.Earliest;
                    e.ConfigureConsumer<NasdaqConsumer>(ctx);
                });
            });
        });
    });

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
