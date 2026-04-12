using LD.Messaging.Adapter;
using LD.Messaging.Domain.Messages;
using LD.Messaging.FileService.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IStockFileAdapter, StockFileAdapter>();
builder.Services.AddSingleton<StockPublisher>();
builder.Services.AddHostedService<FileMonitorService>();

// ── MassTransit + Kafka ───────────────────────────────────────────────────
// The main bus uses in-memory transport (no broker needed for the bus itself).
// All stock-data publishing goes through the Kafka rider with one topic per exchange.
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();

    x.AddRider(rider =>
    {
        rider.AddProducer<Ftse500Data>("FTSE500");
        rider.AddProducer<NyseData>("NYSE");
        rider.AddProducer<NasdaqData>("NASDAQ");

        rider.UsingKafka((_, k) =>
        {
            k.Host(builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092");
        });
    });
});

await builder.Build().RunAsync();
