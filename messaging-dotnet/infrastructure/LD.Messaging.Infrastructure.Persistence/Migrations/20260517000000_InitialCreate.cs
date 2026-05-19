using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LD.Messaging.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ingestion");

            migrationBuilder.CreateTable(
                name: "stock_records",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Stock ticker symbol"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Company or exchange name"),
                    open = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false, comment: "Opening price"),
                    high = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false, comment: "Highest price"),
                    low = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false, comment: "Lowest price"),
                    close = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false, comment: "Closing price"),
                    volume = table.Column<long>(type: "bigint", nullable: false, comment: "Trading volume"),
                    change_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, comment: "Percentage change"),
                    exchange = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Market exchange (FTSE500, NYSE, NASDAQ, etc.)"),
                    record_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "Date the record was recorded"),
                    record_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false, comment: "Time the record was recorded"),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Audit trail: source file name"),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'", comment: "Timestamp when persisted (UTC)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_records", x => x.id);
                },
                comment: "Stock market data records ingested from Kafka");

            migrationBuilder.CreateIndex(
                name: "idx_stock_records_created",
                schema: "ingestion",
                table: "stock_records",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "idx_stock_records_exchange",
                schema: "ingestion",
                table: "stock_records",
                column: "exchange");

            migrationBuilder.CreateIndex(
                name: "idx_stock_records_date_exchange",
                schema: "ingestion",
                table: "stock_records",
                columns: new[] { "record_date", "exchange" });

            migrationBuilder.CreateIndex(
                name: "idx_stock_records_symbol",
                schema: "ingestion",
                table: "stock_records",
                column: "symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_records",
                schema: "ingestion");
        }
    }
}

