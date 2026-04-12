namespace LD.Messaging.Domain.Messages;

/// <summary>Kafka message published to the NASDAQ topic.</summary>
public record NasdaqData(
    DateOnly Date,
    TimeOnly Time,
    IReadOnlyList<StockRecord> Records,
    string FileName
);
