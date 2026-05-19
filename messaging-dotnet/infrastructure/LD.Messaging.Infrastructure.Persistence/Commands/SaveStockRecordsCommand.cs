using LD.Messaging.Domain;

namespace LD.Messaging.Infrastructure.Persistence.Commands;

/// <summary>
/// Command to save stock records to the database.
/// 
/// This command encapsulates the operation of persisting stock records
/// with their metadata (source exchange, date, time, file name).
/// 
/// CQRS Pattern: This is a command (write operation) that changes the state of the system.
/// </summary>
public sealed record SaveStockRecordsCommand(
    /// <summary>Collection of stock records to persist.</summary>
    IReadOnlyList<StockRecord> Records,
    
    /// <summary>The market exchange these records originated from (FTSE500, NYSE, NASDAQ).</summary>
    string Exchange,
    
    /// <summary>Date the records were recorded.</summary>
    DateOnly RecordDate,
    
    /// <summary>Time the records were recorded.</summary>
    TimeOnly RecordTime,
    
    /// <summary>Name of the file these records were ingested from (for audit trail).</summary>
    string FileName
);

