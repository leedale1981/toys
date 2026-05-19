using LD.Messaging.Domain;
using LD.Messaging.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LD.Messaging.Infrastructure.Persistence.Commands;

/// <summary>
/// Handles the SaveStockRecordsCommand by persisting records to PostgreSQL.
/// 
/// Security Considerations:
/// - Uses EF Core which automatically generates parameterized queries (prevents SQL injection)
/// - All user input (exchange, dates, file names, stock data) is passed as parameters, never as SQL strings
/// - No string concatenation is performed for SQL queries
/// - Input validation is performed before database operations
/// </summary>
public sealed class SaveStockRecordsCommandHandler(
    StockRecordsDbContext dbContext,
    ILogger<SaveStockRecordsCommandHandler> logger)
{
    private readonly StockRecordsDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ILogger<SaveStockRecordsCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Executes the command to save stock records to the database.
    /// 
    /// The operation is atomic: all records are saved in a single transaction.
    /// If any error occurs, the entire batch is rolled back.
    /// </summary>
    public async Task HandleAsync(SaveStockRecordsCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        // Validate command inputs
        ValidateCommand(command);

        _logger.LogInformation(
            "Persisting {Count} stock records from {Exchange} (Date: {Date}, Time: {Time}, File: {FileName})",
            command.Records.Count,
            command.Exchange,
            command.RecordDate,
            command.RecordTime,
            command.FileName);

        try
        {
            // Begin transaction for atomic operation
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Map domain records to database entities using extension method
            // (see StockRecordMappingExtensions for mapping logic)
            var entities = command.Records.ToEntities(command);

            // Add entities to DbSet (still in memory, not yet committed)
            await _dbContext.StockRecords.AddRangeAsync(entities, cancellationToken);

            // Execute INSERT statements with parameterized queries
            // EF Core generates SQL like:
            // INSERT INTO ingestion.stock_records (symbol, name, open, high, low, close, volume, change_percent, exchange, record_date, record_time, file_name, created_at_utc)
            // VALUES (@p0, @p1, @p2, ... ) with actual values passed as parameters
            int rowsAffected = await _dbContext.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully persisted {RowsAffected} stock records to database",
                rowsAffected);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to persist stock records from {Exchange}: {ErrorMessage}",
                command.Exchange,
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Validates the command to ensure all required fields are present and valid.
    /// This prevents invalid data from reaching the database.
    /// </summary>
    private static void ValidateCommand(SaveStockRecordsCommand command)
    {
        if (command.Records == null || command.Records.Count == 0)
            throw new ArgumentException("Records collection cannot be null or empty", nameof(command));

        if (string.IsNullOrWhiteSpace(command.Exchange))
            throw new ArgumentException("Exchange cannot be null or empty", nameof(command.Exchange));

        if (string.IsNullOrWhiteSpace(command.FileName))
            throw new ArgumentException("FileName cannot be null or empty", nameof(command.FileName));

        // Validate exchange is one of the known values
        var validExchanges = new[] { "FTSE500", "NYSE", "NASDAQ" };
        if (!validExchanges.Contains(command.Exchange))
            throw new ArgumentException(
                $"Exchange must be one of: {string.Join(", ", validExchanges)}",
                nameof(command.Exchange));

        // Validate individual stock records
        try
        {
            foreach (var (record, index) in command.Records.Select((r, i) => (r, i)))
            {
                ValidateStockRecord(record, index);
            }
        }
        catch (ArgumentException)
        {
            throw;
        }
    }

    /// <summary>
    /// Validates individual stock record data.
    /// </summary>
    private static void ValidateStockRecord(StockRecord record, int index)
    {
        if (string.IsNullOrWhiteSpace(record.Symbol))
        {
            throw new ArgumentException($"Record {index}: Symbol cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(record.Name))
        {
            throw new ArgumentException($"Record {index}: Name cannot be null or empty");
        }

        if (record.Symbol.Length > 10)
        {
            throw new ArgumentException($"Record {index}: Symbol exceeds maximum length of 10 characters");
        }

        if (record.Name.Length > 255)
        {
            throw new ArgumentException($"Record {index}: Name exceeds maximum length of 255 characters");
        }

        if (record.Open < 0 || record.High < 0 || record.Low < 0 || record.Close < 0)
        {
            throw new ArgumentException($"Record {index}: Price values cannot be negative");
        }

        if (record.Volume < 0)
        {
            throw new ArgumentException($"Record {index}: Volume cannot be negative");
        }

        if (record.ChangePercent < -100 || record.ChangePercent > 1000)
        {
            throw new ArgumentException($"Record {index}: ChangePercent is outside reasonable bounds");
        }
    }
}

