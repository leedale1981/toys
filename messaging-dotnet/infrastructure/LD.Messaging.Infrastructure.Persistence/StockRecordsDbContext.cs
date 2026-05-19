using LD.Messaging.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LD.Messaging.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for stock records persistence to PostgreSQL.
/// 
/// All database queries are parameterized via EF Core, preventing SQL injection attacks.
/// </summary>
public sealed class StockRecordsDbContext : DbContext
{
    public DbSet<StockRecordEntity> StockRecords => Set<StockRecordEntity>();

    public StockRecordsDbContext(DbContextOptions<StockRecordsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StockRecordEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configure columns
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Symbol)
                .IsRequired()
                .HasMaxLength(10)
                .HasComment("Stock ticker symbol");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Company or exchange name");

            entity.Property(e => e.Open)
                .HasPrecision(19, 4)
                .HasComment("Opening price");

            entity.Property(e => e.High)
                .HasPrecision(19, 4)
                .HasComment("Highest price");

            entity.Property(e => e.Low)
                .HasPrecision(19, 4)
                .HasComment("Lowest price");

            entity.Property(e => e.Close)
                .HasPrecision(19, 4)
                .HasComment("Closing price");

            entity.Property(e => e.Volume)
                .HasComment("Trading volume");

            entity.Property(e => e.ChangePercent)
                .HasPrecision(5, 2)
                .HasComment("Percentage change");

            entity.Property(e => e.Exchange)
                .IsRequired()
                .HasMaxLength(20)
                .HasComment("Market exchange (FTSE500, NYSE, NASDAQ, etc.)");

            entity.Property(e => e.RecordDate)
                .IsRequired()
                .HasComment("Date the record was recorded");

            entity.Property(e => e.RecordTime)
                .IsRequired()
                .HasComment("Time the record was recorded");

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Audit trail: source file name");

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
                .HasComment("Timestamp when persisted (UTC)");

            // Create indexes for common queries
            entity.HasIndex(e => e.Symbol)
                .HasDatabaseName("idx_stock_records_symbol");

            entity.HasIndex(e => e.Exchange)
                .HasDatabaseName("idx_stock_records_exchange");

            entity.HasIndex(e => new { e.RecordDate, e.Exchange })
                .HasDatabaseName("idx_stock_records_date_exchange");

            entity.HasIndex(e => e.CreatedAtUtc)
                .HasDatabaseName("idx_stock_records_created");

            // Table name and schema
            entity.ToTable("stock_records", "ingestion");
        });
    }
}

