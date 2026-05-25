using LD.Messaging.Domain;
using LD.Messaging.DataInterfaces.Commands;

namespace LD.Messaging.DataInterfaces.StockRecords;

/// <summary>
/// CQRS command to persist a batch of stock records to the database.
///
/// CQRS Pattern: This is a write-only command — it changes state and returns nothing.
/// All records in the batch share the same exchange, date, time, and source file.
/// </summary>
/// <param name="Records">Collection of stock records to persist.</param>
/// <param name="Exchange">The market exchange these records originated from (FTSE500, NYSE, NASDAQ).</param>
/// <param name="RecordDate">Date the records were recorded.</param>
/// <param name="RecordTime">Time the records were recorded.</param>
/// <param name="FileName">Name of the source file (audit trail).</param>
public sealed record SaveStockRecordsCommand(
    IReadOnlyList<StockRecord> Records,
    string Exchange,
    DateOnly RecordDate,
    TimeOnly RecordTime,
    string FileName
) : ICommand;

