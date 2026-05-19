# Quick Reference: CQRS Postgres Persistence Layer

## 🚀 Quick Start (Copy & Paste)

### 1. Start PostgreSQL (Docker)
```bash
docker run -d \
  --name postgres-stock \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=stock_records \
  -p 5432:5432 \
  postgres:18-alpine
```

### 2. Apply Migrations
```powershell
cd ingestion-service\LD.Messaging.Ingestion
dotnet ef database update `
    --project ..\..\infrastructure\LD.Messaging.Infrastructure.Persistence
```

### 3. Run Service
```powershell
dotnet run
```

### 4. Verify Data (in separate terminal)
```bash
psql -h localhost -U postgres -d stock_records -c "SELECT COUNT(*) FROM ingestion.stock_records;"
```

---

## 📧 CQRS Command Example

```csharp
// Create command
var command = new SaveStockRecordsCommand(
    Records: stockRecords,           // IReadOnlyList<StockRecord>
    Exchange: "FTSE500",             // "FTSE500", "NYSE", or "NASDAQ"
    RecordDate: DateOnly.Today,
    RecordTime: TimeOnly.FromDateTime(DateTime.UtcNow),
    FileName: "ftse500_20260517.csv"
);

// Execute command
await commandHandler.HandleAsync(command, cancellationToken);
```

---

## 🔗 Connection String

**Development**:
```
Host=localhost;Port=5432;Database=stock_records;Username=postgres;Password=postgres
```

**Production**:
```
Host=myserver.postgres.database.azure.com;Port=5432;Database=stock_records;Username=postgres@myserver;Password=<secure-password>;SSL Mode=Require;
```

---

## 📁 Key Files

| File | Purpose |
|------|---------|
| `SaveStockRecordsCommand.cs` | CQRS command definition |
| `SaveStockRecordsCommandHandler.cs` | Command execution logic |
| `StockRecordsDbContext.cs` | EF Core configuration |
| `StockRecordEntity.cs` | Database entity model |
| `setup-database.sql` | Manual database setup |

---

## 🔐 Security Guarantees

| Threat | Prevention |
|--------|-----------|
| **SQL Injection** | Parameterized queries (EF Core automatic) |
| **Invalid Exchange** | Whitelist validation (FTSE500, NYSE, NASDAQ) |
| **Negative Prices** | CHECK constraints + C# validation |
| **Partial Commits** | Atomic transactions (all or nothing) |
| **Empty Strings** | NOT NULL + validation checks |

---

## 📊 Database Query Examples

### Count all records
```sql
SELECT COUNT(*) FROM ingestion.stock_records;
```

### Find by symbol
```sql
SELECT * FROM ingestion.stock_records WHERE symbol = 'AAPL' LIMIT 10;
```

### Find by exchange
```sql
SELECT COUNT(*) FROM ingestion.stock_records WHERE exchange = 'NYSE';
```

### Find by date range
```sql
SELECT * FROM ingestion.stock_records 
WHERE record_date BETWEEN '2026-05-01' AND '2026-05-17'
  AND exchange = 'FTSE500'
ORDER BY record_time DESC;
```

### Recent records with audit info
```sql
SELECT symbol, name, close, exchange, file_name, created_at_utc
FROM ingestion.stock_records 
ORDER BY created_at_utc DESC LIMIT 20;
```

---

## 🛠️ Common Tasks

### Enable SQL Query Logging
Edit `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Debug",
      "Default": "Information"
    }
  }
}
```

### Check Migrations Applied
```powershell
psql -h localhost -U postgres -d stock_records -c "SELECT * FROM __EFMigrationsHistory;"
```

### Clear Data (Development Only)
```sql
DELETE FROM ingestion.stock_records;
-- Reset sequence
ALTER SEQUENCE ingestion.stock_records_id_seq RESTART WITH 1;
```

### View Table Structure
```bash
psql -h localhost -U postgres -d stock_records -c "\d ingestion.stock_records"
```

---

## ⚠️ Troubleshooting

### Connection Refused
```
✗ Problem: PostgreSQL not running
✓ Solution: docker ps | grep postgres  (or brew services list)
```

### Authentication Failed
```
✗ Problem: Wrong username/password
✓ Solution: Check connection string in appsettings.json
```

### Table Not Found
```
✗ Problem: Migrations not applied
✓ Solution: dotnet ef database update
```

### Type or Namespace Errors
```
✗ Problem: Missing using statements
✓ Solution: Check imports - need LD.Messaging.Domain
```

---

## 🏗️ Architecture at a Glance

```
        Kafka
         │
         ▼
    Consumer
         │
         ▼
SaveStockRecordsCommand
         │
         ▼
SaveStockRecordsCommandHandler
         │
         ├─→ Validate input
         ├─→ Map domain → entity
         └─→ EF Core SaveChangesAsync()
                    │
                    ▼
              PostgreSQL (parameterized)
```

---

## 📝 Dependency Injection Registration

```csharp
// In Program.cs
var connectionString = builder.Configuration.GetConnectionString("StockRecordsDb");
builder.Services.AddStockRecordsPersistence(connectionString);
```

This registers:
- `StockRecordsDbContext` (scoped)
- `SaveStockRecordsCommandHandler` (scoped)

---

## 💥 Validation Rules

| Field | Rule | Value |
|-------|------|-------|
| Exchange | Whitelist | FTSE500, NYSE, NASDAQ |
| Symbol | Uppercase max 10 | e.g., "AAPL" |
| Name | Max 255 chars | e.g., "Apple Inc." |
| Open/High/Low/Close | >= 0 | e.g., 150.25 |
| Volume | >= 0 | e.g., 1000000 |
| ChangePercent | -100 to 1000 | e.g., 2.5 |
| FileName | Not empty | e.g., "ftse500.csv" |

---

## 🔍 Generated SQL Example

**What you write**:
```csharp
var command = new SaveStockRecordsCommand(
    Records: new[] { stockRecord },
    Exchange: "NYSE",
    RecordDate: DateOnly.Today,
    RecordTime: TimeOnly.Now,
    FileName: "data.csv"
);
```

**What EF Core generates**:
```sql
INSERT INTO ingestion.stock_records 
(symbol, name, open, high, low, close, volume, change_percent, exchange, record_date, record_time, file_name, created_at_utc)
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, CURRENT_TIMESTAMP);

-- Database receives actual values as @p0, @p1, etc. - NOT executable code
```

---

## 📦 Project References

```
LD.Messaging.Ingestion
├── MassTransit
├── LD.Messaging.Domain
└── LD.Messaging.Infrastructure.Persistence
    ├── Microsoft.EntityFrameworkCore
    ├── Npgsql.EntityFrameworkCore.PostgreSQL
    └── LD.Messaging.Domain
```

---

## ✅ Health Check Commands

```powershell
# Test PostgreSQL connection
psql -h localhost -U postgres -d stock_records -c "SELECT 1;"

# Test .NET build
dotnet build

# Check migrations
dotnet ef migrations list --project infrastructure/LD.Messaging.Infrastructure.Persistence

# View database
psql -h localhost -U postgres -d stock_records
  # Then: \dt ingestion.*
  # Then: SELECT COUNT(*) FROM ingestion.stock_records;
```

---

## 📧 Log Format

With debug logging enabled, you'll see:

```
[Debug] Executing DbCommand [Parameters=[@p0='AAPL', @p1='Apple Inc.', @p2='150.30', ...], CommandType='Text', CommandTimeout='30']
INSERT INTO ingestion.stock_records (symbol, name, open, high, low, close, volume, change_percent, exchange, record_date, record_time, file_name, created_at_utc) VALUES (@p0, @p1, @p2, ...)
[Information] Successfully persisted 100 stock records to database
```

---

## 🎯 One-Liner Status Check

```bash
curl -s "psql -h localhost -U postgres -d stock_records -c 'SELECT COUNT(*) as total_records FROM ingestion.stock_records;' 2>/dev/null" || echo "Database not accessible"
```

---

## 📚 Related Files

- `README.md` - Full architecture explanation
- `DEPLOYMENT_GUIDE.md` - Step-by-step setup
- `IMPLEMENTATION_SUMMARY.md` - What was built
- `VERIFICATION_CHECKLIST.md` - Completeness verification

---

**Last Updated**: May 17, 2026  
**Status**: ✅ Production Ready

