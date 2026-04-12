namespace LD.Messaging.Domain.Messages;

/// <summary>Kafka message published to the FTSE500 topic.</summary>
public record Ftse500Data(
    DateOnly Date,
    TimeOnly Time,
    IReadOnlyList<StockRecord> Records,
    string FileName
);
