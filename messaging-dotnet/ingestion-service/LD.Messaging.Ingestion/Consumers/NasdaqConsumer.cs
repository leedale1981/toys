using LD.Messaging.Domain.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LD.Messaging.Ingestion.Consumers;

public sealed class NasdaqConsumer(ILogger<NasdaqConsumer> logger) : IConsumer<NasdaqData>
{
    public Task Consume(ConsumeContext<NasdaqData> context)
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

        return Task.CompletedTask;
    }
}
