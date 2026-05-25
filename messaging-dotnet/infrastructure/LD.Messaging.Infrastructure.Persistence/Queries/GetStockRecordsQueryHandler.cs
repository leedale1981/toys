using LD.Messaging.DataInterfaces.Queries;
using LD.Messaging.DataInterfaces.StockRecords;
using LD.Messaging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LD.Messaging.Infrastructure.Persistence.Queries;

/// <summary>
/// Handles <see cref="GetStockRecordsQuery"/> by reading from PostgreSQL via EF Core.
/// Implements <see cref="IQueryHandler{TQuery,TResult}"/> — callers depend only on the interface.
/// </summary>
public sealed class GetStockRecordsQueryHandler(
    StockRecordsDbContext dbContext,
    ILogger<GetStockRecordsQueryHandler> logger)
    : IQueryHandler<GetStockRecordsQuery, IReadOnlyList<StockRecord>>
{
    public async Task<IReadOnlyList<StockRecord>> HandleAsync(
        GetStockRecordsQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Querying stock records — Symbol: {Symbol}, Exchange: {Exchange}, Date: {Date}, PageSize: {PageSize}, Offset: {Offset}",
            query.Symbol ?? "any",
            query.Exchange ?? "any",
            query.RecordDate?.ToString("yyyy-MM-dd") ?? "any",
            query.PageSize,
            query.PageOffset);

        var q = dbContext.StockRecords.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Symbol))
        {
            q = q.Where(e => e.Symbol == query.Symbol);
        }

        if (!string.IsNullOrWhiteSpace(query.Exchange))
        {
            q = q.Where(e => e.Exchange == query.Exchange);
        }

        if (query.RecordDate.HasValue)
        {
            q = q.Where(e => e.RecordDate == query.RecordDate.Value);
        }

        var entities = await q
            .OrderByDescending(e => e.RecordDate)
            .ThenByDescending(e => e.CreatedAtUtc)
            .Skip(query.PageOffset)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Query returned {Count} stock records", entities.Count);

        return entities
            .Select(e => new StockRecord(
                e.Symbol,
                e.Name,
                e.Open,
                e.High,
                e.Low,
                e.Close,
                e.Volume,
                e.ChangePercent))
            .ToList();
    }
}

