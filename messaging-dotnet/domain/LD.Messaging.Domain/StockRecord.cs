namespace LD.Messaging.Domain;

public record StockRecord(
    string Symbol,
    string Name,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume,
    decimal ChangePercent
);
