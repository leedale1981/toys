using Confluent.Kafka;
using LD.Messaging.Domain.Messages;
using LD.Messaging.Ingestion.Consumers;
using LD.Messaging.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add persistence layer with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("StockRecordsDb")
    ?? throw new InvalidOperationException("Connection string 'StockRecordsDb' not found in configuration");

builder.Services.AddStockRecordsPersistence(connectionString);

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
