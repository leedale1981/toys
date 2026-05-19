using LD.Messaging.Domain.Messages;
using LD.Messaging.Infrastructure.Persistence.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LD.Messaging.Ingestion.Consumers;

public sealed class Ftse500Consumer(
    SaveStockRecordsCommandHandler commandHandler,
    ILogger<Ftse500Consumer> logger) : IConsumer<Ftse500Data>
{
    public async Task Consume(ConsumeContext<Ftse500Data> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "[FTSE500] {FileName} | {Date} {Time} | {Count} records received",
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
            Exchange: "FTSE500",
            RecordDate: msg.Date,
            RecordTime: msg.Time,
            FileName: msg.FileName);

        await commandHandler.HandleAsync(command, context.CancellationToken);

        logger.LogInformation("[FTSE500] Successfully persisted {Count} records to database", msg.Records.Count);
    }
}
