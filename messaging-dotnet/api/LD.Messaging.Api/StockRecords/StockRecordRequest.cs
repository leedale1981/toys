namespace LD.Messaging.Api.StockRecords;

/// <summary>
/// Query parameters bound from the HTTP query string for GET /api/stockrecords.
/// All filters are optional — omitting them returns all records (subject to pagination).
/// </summary>
public class StockRecordRequest
{
    /// <summary>Filter by ticker symbol (e.g. "AAPL").</summary>
    public string? Symbol { get; set; }

    /// <summary>Filter by exchange (e.g. "NYSE", "NASDAQ", "FTSE500").</summary>
    public string? Exchange { get; set; }

    /// <summary>Filter by record date (ISO-8601: yyyy-MM-dd).</summary>
    public DateOnly? RecordDate { get; set; }

    /// <summary>Maximum number of records to return (default 100, max 1000).</summary>
    public int PageSize { get; set; } = 100;

    /// <summary>Number of records to skip for pagination (default 0).</summary>
    public int PageOffset { get; set; } = 0;
}