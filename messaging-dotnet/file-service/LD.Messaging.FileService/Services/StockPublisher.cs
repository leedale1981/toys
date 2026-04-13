using LD.Messaging.Domain;
using LD.Messaging.Domain.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LD.Messaging.FileService.Services;

/// <summary>
/// Routes parsed stock data to the correct Kafka topic via MassTransit
/// <see cref="ITopicProducer{T}"/>. Each exchange maps to a dedicated message
/// type and topic: FTSE500 → <see cref="Ftse500Data"/>, NYSE → <see cref="NyseData"/>,
/// NASDAQ → <see cref="NasdaqData"/>.
/// </summary>
/// <remarks>
/// Producers are resolved lazily from the service provider on first use rather than
/// via constructor injection. MassTransit's ITopicProducer&lt;T&gt; factory calls
/// GetRider&lt;IKafkaRider&gt;() at resolve time, which throws if the rider has not
/// yet started. Since this class is a singleton it would otherwise be instantiated
/// before any hosted service (including the MassTransit bus) has started.
/// </remarks>
public sealed class StockPublisher(IServiceProvider services, ILogger<StockPublisher> logger)
{
    private ITopicProducer<Ftse500Data>? _ftseProducer;
    private ITopicProducer<NyseData>? _nyseProducer;
    private ITopicProducer<NasdaqData>? _nasdaqProducer;

    public async Task PublishAsync(StockMarketData data, string fileName, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Publishing {Exchange} data ({Count} records) to Kafka",
            data.Exchange, data.Records.Count);

        switch (data.Exchange)
        {
            case Exchange.FTSE500:
                _ftseProducer ??= services.GetRequiredService<ITopicProducer<Ftse500Data>>();
                await _ftseProducer.Produce(
                    new Ftse500Data(data.Date, data.Time, data.Records, fileName), ct);
                break;

            case Exchange.NYSE:
                _nyseProducer ??= services.GetRequiredService<ITopicProducer<NyseData>>();
                await _nyseProducer.Produce(
                    new NyseData(data.Date, data.Time, data.Records, fileName), ct);
                break;

            case Exchange.NASDAQ:
                _nasdaqProducer ??= services.GetRequiredService<ITopicProducer<NasdaqData>>();
                await _nasdaqProducer.Produce(
                    new NasdaqData(data.Date, data.Time, data.Records, fileName), ct);
                break;

            default:
                logger.LogWarning("No Kafka topic configured for exchange {Exchange}", data.Exchange);
                break;
        }
    }
}
