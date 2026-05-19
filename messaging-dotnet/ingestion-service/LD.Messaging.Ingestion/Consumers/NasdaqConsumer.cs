using LD.Messaging.Domain.Messages;
using LD.Messaging.Infrastructure.Persistence.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LD.Messaging.Ingestion.Consumers;

public sealed class NasdaqConsumer(
    SaveStockRecordsCommandHandler commandHandler,
    ILogger<NasdaqConsumer> logger) : IConsumer<NasdaqData>
{
    public async Task Consume(ConsumeContext<NasdaqData> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "[NASDAQ] {FileName} | {Date} {Time} | {Count} records received",
            msg.FileName, msg.Date, msg.Time, msg.Records.Count);

        foreach (var record in msg.Records)
        {
            logger.LogDebug(
                "  {Symbol} ({Name}) — Close: {Close:F2}  Change: {Change:+0.00;-0.00}%  Vol: {Volume:N0}",
                record.Symbol, record.Name, record.Close, record.ChangePercent, record.Volume);
        }

        // Execute CQRS command to persist stock records to PostgreSQL
        var command = new SaveStockRecordsCommand(
            Records: msg.Records,
            Exchange: "NASDAQ",
            RecordDate: msg.Date,
            RecordTime: msg.Time,
            FileName: msg.FileName);

        await commandHandler.HandleAsync(command, context.CancellationToken);

        logger.LogInformation("[NASDAQ] Successfully persisted {Count} records to database", msg.Records.Count);
    }
}
