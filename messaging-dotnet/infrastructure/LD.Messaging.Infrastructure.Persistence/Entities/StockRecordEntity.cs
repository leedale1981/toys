namespace LD.Messaging.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a stock record persisted to the database.
/// This entity maps to the stock_records table in PostgreSQL.
/// </summary>
public class StockRecordEntity
{
    /// <summary>Auto-generated primary key.</summary>
    public int Id { get; set; }

    /// <summary>Stock ticker symbol (e.g., "AAPL", "MSFT"). Indexed for fast lookups.</summary>
    public string Symbol { get; set; } = null!;

    /// <summary>Company or exchange name.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Opening price for the period.</summary>
    public decimal Open { get; set; }

    /// <summary>Highest price for the period.</summary>
    public decimal High { get; set; }

    /// <summary>Lowest price for the period.</summary>
    public decimal Low { get; set; }

    /// <summary>Closing price for the period.</summary>
    public decimal Close { get; set; }

    /// <summary>Trading volume (number of shares traded).</summary>
    public long Volume { get; set; }

    /// <summary>Percentage change from open to close.</summary>
    public decimal ChangePercent { get; set; }

    /// <summary>The market exchange (FTSE500, NYSE, NASDAQ, etc.).</summary>
    public string Exchange { get; set; } = null!;

    /// <summary>Date the recorded was recorded.</summary>
    public DateOnly RecordDate { get; set; }

    /// <summary>Time the record was recorded.</summary>
    public TimeOnly RecordTime { get; set; }

    /// <summary>Name of the file this record was ingested from (for audit trail).</summary>
    public string FileName { get; set; } = null!;

    /// <summary>Timestamp when the record was persisted to the database (UTC).</summary>
    public DateTime CreatedAtUtc { get; set; }
}

