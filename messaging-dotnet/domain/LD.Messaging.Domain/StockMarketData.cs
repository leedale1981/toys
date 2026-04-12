namespace LD.Messaging.Domain;

public record StockMarketData(
    Exchange Exchange,
    DateOnly Date,
    TimeOnly Time,
    IReadOnlyList<StockRecord> Records
);
