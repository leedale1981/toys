using LD.Messaging.Domain;
using LD.Messaging.Domain.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LD.Messaging.FileService.Services;

/// <summary>
/// Routes parsed stock data to the correct Kafka topic via MassTransit
/// <see cref="ITopicProducer{T}"/>. Each exchange maps to a dedicated message
/// type and topic: FTSE500 → <see cref="Ftse500Data"/>, NYSE → <see cref="NyseData"/>,
/// NASDAQ → <see cref="NasdaqData"/>.
/// </summary>
public sealed class StockPublisher(
    ITopicProducer<Ftse500Data> ftseProducer,
    ITopicProducer<NyseData> nyseProducer,
    ITopicProducer<NasdaqData> nasdaqProducer,
    ILogger<StockPublisher> logger)
{
    public async Task PublishAsync(StockMarketData data, string fileName, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Publishing {Exchange} data ({Count} records) to Kafka",
            data.Exchange, data.Records.Count);

        switch (data.Exchange)
        {
            case Exchange.FTSE500:
                await ftseProducer.Produce(
                    new Ftse500Data(data.Date, data.Time, data.Records, fileName), ct);
                break;

            case Exchange.NYSE:
                await nyseProducer.Produce(
                    new NyseData(data.Date, data.Time, data.Records, fileName), ct);
                break;

            case Exchange.NASDAQ:
                await nasdaqProducer.Produce(
                    new NasdaqData(data.Date, data.Time, data.Records, fileName), ct);
                break;

            default:
                logger.LogWarning("No Kafka topic configured for exchange {Exchange}", data.Exchange);
                break;
        }
    }
}
