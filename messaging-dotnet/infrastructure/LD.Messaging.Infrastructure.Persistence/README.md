# PostgreSQL Persistence Layer - CQRS Implementation

## Overview

This document describes the PostgreSQL persistence layer for the ingestion service, implemented using CQRS (Command Query Responsibility Segregation) pattern and Entity Framework Core with proper security controls.

## Architecture

### CQRS Pattern

The persistence layer follows the CQRS pattern:

- **Command**: `SaveStockRecordsCommand` - Represents the intent to persist stock records
- **Command Handler**: `SaveStockRecordsCommandHandler` - Executes the command and persists data

### Data Flow

```
Kafka Message (Ftse500Data, NyseData, NasdaqData)
         ↓
   Kafka Consumer (MassTransit)
         ↓
   Create SaveStockRecordsCommand
         ↓
   SaveStockRecordsCommandHandler.HandleAsync()
         ↓
   Validate Command Input
         ↓
   EF Core DbContext (parameterized queries)
         ↓
   PostgreSQL 18 Database
         ↓
   ingestion.stock_records table
```

## Security Implementation

### SQL Injection Prevention

**Threat Model**: User input could contain malicious SQL if concatenated into queries.

**Mitigation**:
1. **Parameterized Queries**: EF Core automatically generates parameterized SQL
   - No string concatenation for SQL
   - All values passed as parameters (e.g., `@p0`, `@p1`, etc.)
   - Database engine treats values as data, never as code

2. **Example Generated Query** (EF Core):
   ```sql
   INSERT INTO ingestion.stock_records 
   (symbol, name, open, high, low, close, volume, change_percent, exchange, record_date, record_time, file_name, created_at_utc)
   VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12);
   ```

3. **Input Validation** in `SaveStockRecordsCommandHandler`:
   - Exchange validation (whitelist: FTSE500, NYSE, NASDAQ)
   - Symbol length validation (max 10 chars)
   - Price range checks
   - Volume non-negative checks
   - String trimming to remove whitespace attacks

### Other Security Considerations

1. **Input Validation Layers**:
   - Command validation before database operation
   - EF Core column constraints (CHECK constraints)
   - Database-level constraints

2. **Transactional Integrity**:
   - All records inserted atomically
   - Rollback on any error
   - No partial commits

3. **Audit Trail**:
   - `FileName` field tracks source of data
   - `CreatedAtUtc` timestamp for ingestion tracking
   - `Exchange` identifies market source

## Project Structure

```
infrastructure/
└── LD.Messaging.Infrastructure.Persistence/
    ├── LD.Messaging.Infrastructure.Persistence.csproj
    ├── StockRecordsDbContext.cs              # EF Core DbContext
    ├── ServiceCollectionExtensions.cs        # DI registration
    ├── setup-database.sql                    # Manual setup script
    ├── Entities/
    │   └── StockRecordEntity.cs             # DB entity model
    ├── Migrations/
    │   ├── 20260517000000_InitialCreate.cs
    │   ├── 20260517000000_InitialCreate.Designer.cs
    │   └── StockRecordsDbContextModelSnapshot.cs
    └── Commands/
        ├── SaveStockRecordsCommand.cs        # CQRS command
        └── SaveStockRecordsCommandHandler.cs # Command handler
```

## Database Schema

### Table: `ingestion.stock_records`

```sql
CREATE TABLE ingestion.stock_records (
    id SERIAL PRIMARY KEY,
    symbol VARCHAR(10) NOT NULL,
    name VARCHAR(255) NOT NULL,
    open NUMERIC(19, 4) NOT NULL,
    high NUMERIC(19, 4) NOT NULL,
    low NUMERIC(19, 4) NOT NULL,
    close NUMERIC(19, 4) NOT NULL,
    volume BIGINT NOT NULL,
    change_percent NUMERIC(5, 2) NOT NULL,
    exchange VARCHAR(20) NOT NULL,
    record_date DATE NOT NULL,
    record_time TIME NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    created_at_utc TIMESTAMP WITH TIME ZONE NOT NULL
);
```

### Indexes

| Index | Columns | Purpose |
|-------|---------|---------|
| `idx_stock_records_symbol` | symbol | Fast lookups by stock ticker |
| `idx_stock_records_exchange` | exchange | Market-specific queries |
| `idx_stock_records_date_exchange` | (record_date, exchange) | Time-range queries |
| `idx_stock_records_created` | created_at_utc DESC | Auditing and retention |

### Constraints

| Type | Constraint | Purpose |
|------|-----------|---------|
| CHECK | `exchange IN ('FTSE500', 'NYSE', 'NASDAQ')` | Restrict to known exchanges |
| CHECK | `symbol <> ''` | Prevent empty symbols |
| CHECK | `open >= 0, high >= 0, ...` | Ensure non-negative prices |
| CHECK | `change_percent BETWEEN -100 AND 1000` | Reasonable change bounds |

## Configuration

### Connection String

Set in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "StockRecordsDb": "Host=localhost;Port=5432;Database=stock_records;Username=postgres;Password=postgres"
  }
}
```

**For Production**:
- Use strong passwords
- Use role-based access control
- Use SSL/TLS connection encryption
- Consider managed PostgreSQL service with encryption at rest

### Dependency Injection

In `Program.cs`:

```csharp
using LD.Messaging.Infrastructure.Persistence;

var connectionString = builder.Configuration.GetConnectionString("StockRecordsDb");
builder.Services.AddStockRecordsPersistence(connectionString);
```

This registers:
- `StockRecordsDbContext` (scoped)
- `SaveStockRecordsCommandHandler` (scoped)

## Usage Example

### In a Consumer

```csharp
public sealed class Ftse500Consumer(
    SaveStockRecordsCommandHandler commandHandler,
    ILogger<Ftse500Consumer> logger) : IConsumer<Ftse500Data>
{
    public async Task Consume(ConsumeContext<Ftse500Data> context)
    {
        var msg = context.Message;
        
        // Create CQRS command
        var command = new SaveStockRecordsCommand(
            Records: msg.Records,
            Exchange: "FTSE500",
            RecordDate: msg.Date,
            RecordTime: msg.Time,
            FileName: msg.FileName);
        
        // Execute command (persists to PostgreSQL)
        await commandHandler.HandleAsync(command, context.CancellationToken);
    }
}
```

## Database Setup

### Option 1: Automatic (EF Core Migrations)

```powershell
# Apply migrations to existing database
dotnet ef database update --project infrastructure/LD.Messaging.Infrastructure.Persistence/LD.Messaging.Infrastructure.Persistence.csproj
```

### Option 2: Manual (SQL Script)

```bash
psql -h localhost -U postgres -d stock_records < setup-database.sql
```

### Docker Setup

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:18-alpine
    environment:
      POSTGRES_DB: stock_records
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      
volumes:
  postgres_data:
```

Run: `docker-compose up -d`

## Performance Considerations

1. **Batch Inserts**: All records from a single message are inserted in one batch (atomic transaction)
2. **Indexes**: Strategic indexes on `symbol`, `exchange`, and `record_date` for common queries
3. **Connection Pooling**: Npgsql automatically manages connection pool
4. **Retry Logic**: EF Core configured with 3 retries on transient failures

## Monitoring and Logging

### Enable SQL Query Logging

In `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Debug"
    }
  }
}
```

This logs all executed SQL queries (use only in development).

### Typical Log Output

```
[Debug] Executing DbCommand [Parameters=[@p0='AAPL', @p1='Apple Inc.', ...], CommandType='Text', CommandTimeout='30']
INSERT INTO ingestion.stock_records (symbol, name, open, high, low, close, volume, change_percent, exchange, record_date, record_time, file_name, created_at_utc) VALUES (@p0, @p1, ...)
```

## Error Handling

### Validation Errors

```csharp
try 
{
    await commandHandler.HandleAsync(command, cancellationToken);
}
catch (ArgumentException ex) 
{
    // Input validation failed
    logger.LogWarning("Validation error: {Message}", ex.Message);
}
catch (DbUpdateException ex) 
{
    // Database operation failed
    logger.LogError(ex, "Database error while persisting records");
}
```

## Future Enhancements

1. **Query Layer**: Add `IStockRecordsQuery` for reading persisted records
2. **Change Data Capture**: Implement CDC for real-time analytics
3. **Partitioning**: Partition `stock_records` by date for better performance
4. **Archive Policy**: Move old records to archive table based on retention policy
5. **Read Replicas**: Add read-only replicas for reporting queries

## References

- EF Core Documentation: https://learn.microsoft.com/en-us/ef/core/
- Npgsql Provider: https://www.npgsql.org/efcore/
- CQRS Pattern: https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs
- PostgreSQL Security: https://www.postgresql.org/docs/current/sql-syntax.html#SQL-SYNTAX-LEXICAL-LITERALS

