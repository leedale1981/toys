using LD.Messaging.Domain;
using LD.Messaging.DataInterfaces.Queries;

namespace LD.Messaging.DataInterfaces.StockRecords;

/// <summary>
/// CQRS query to retrieve stock records from the database.
/// Supports optional filtering by symbol, exchange, and date with pagination.
/// </summary>
/// <param name="Symbol">Optional — filter to a specific ticker symbol (e.g. "AAPL").</param>
/// <param name="Exchange">Optional — filter to a specific exchange (e.g. "NYSE", "NASDAQ", "FTSE500").</param>
/// <param name="RecordDate">Optional — filter to records for a specific date.</param>
/// <param name="PageSize">Maximum number of records to return (default 100).</param>
/// <param name="PageOffset">Number of records to skip for pagination (default 0).</param>
public sealed record GetStockRecordsQuery(
    string? Symbol = null,
    string? Exchange = null,
    DateOnly? RecordDate = null,
    int PageSize = 100,
    int PageOffset = 0
) : IQuery<IReadOnlyList<StockRecord>>;

