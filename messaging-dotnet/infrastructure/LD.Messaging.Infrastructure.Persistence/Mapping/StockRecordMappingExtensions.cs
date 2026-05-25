using LD.Messaging.Domain;
using LD.Messaging.DataInterfaces.StockRecords;
using LD.Messaging.Infrastructure.Persistence.Entities;

namespace LD.Messaging.Infrastructure.Persistence.Mapping;

/// <summary>
/// Extension methods for mapping domain models to database entities.
/// 
/// These methods handle the transformation from domain-layer stock records
/// to database-layer entities. By isolating mapping logic in an extension class,
/// it becomes independently testable and reusable.
/// </summary>
public static class StockRecordMappingExtensions
{
    /// <summary>
    /// Maps a domain StockRecord to a database entity.
    /// 
    /// Security: 
    /// - String values are trimmed to prevent whitespace-based attacks
    /// - Numeric values are validated by caller before mapping
    /// - All values explicitly assigned (no implicit conversions)
    /// 
    /// Preconditions:
    /// - record must not be null
    /// - command must not be null
    /// - record and command data should already be validated
    /// </summary>
    /// <param name="record">Domain StockRecord to map</param>
    /// <param name="command">CQRS command containing metadata (exchange, dates, filename)</param>
    /// <returns>Database entity ready for persistence</returns>
    /// <exception cref="ArgumentNullException">If record or command is null</exception>
    public static StockRecordEntity ToEntity(this StockRecord record, SaveStockRecordsCommand command)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new StockRecordEntity
        {
            Symbol = record.Symbol.Trim(),
            Name = record.Name.Trim(),
            Open = record.Open,
            High = record.High,
            Low = record.Low,
            Close = record.Close,
            Volume = record.Volume,
            ChangePercent = record.ChangePercent,
            Exchange = command.Exchange.Trim(),
            RecordDate = command.RecordDate,
            RecordTime = command.RecordTime,
            FileName = command.FileName.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a collection of domain StockRecords to database entities.
    /// 
    /// Convenience method for bulk mapping with consistent handling.
    /// </summary>
    /// <param name="records">Domain StockRecords to map</param>
    /// <param name="command">CQRS command containing metadata</param>
    /// <returns>List of database entities ready for persistence</returns>
    /// <exception cref="ArgumentNullException">If records or command is null</exception>
    public static List<StockRecordEntity> ToEntities(
        this IEnumerable<StockRecord> records,
        SaveStockRecordsCommand command)
    {
        if (records == null)
            throw new ArgumentNullException(nameof(records));

        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return records
            .Select(record => record.ToEntity(command))
            .ToList();
    }
}

