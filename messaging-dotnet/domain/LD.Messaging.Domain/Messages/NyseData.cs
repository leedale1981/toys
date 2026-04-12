namespace LD.Messaging.Domain.Messages;

/// <summary>Kafka message published to the NYSE topic.</summary>
public record NyseData(
    DateOnly Date,
    TimeOnly Time,
    IReadOnlyList<StockRecord> Records,
    string FileName
);
