using FluentAssertions;
using LD.Messaging.Domain;
using Xunit;

namespace LD.Messaging.Adapter.Tests;

/// <summary>
/// Tests for <see cref="StockFileParser"/> using inline fake file content strings.
/// Each test exercises a distinct phase of the Checked monad validation pipeline.
/// </summary>
public class StockFileParserTests
{
    private readonly StockFileParser _sut = new();

    // ── Shared fake file builders ─────────────────────────────────────────

    private static string ValidFile(
        string exchange = "FTSE500",
        string date = "2026-04-12",
        string time = "09:30:00",
        string columnHeader = "SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT",
        string dataRows = "BP.L,BP PLC,450.20,455.10,449.80,453.60,5234567,0.75") =>
        $"""
        EXCHANGE:{exchange}
        DATE:{date}
        TIME:{time}
        ---
        {columnHeader}
        {dataRows}
        """;

    // ── Happy path ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("FTSE500", Exchange.FTSE500)]
    [InlineData("NYSE",    Exchange.NYSE)]
    [InlineData("NASDAQ",  Exchange.NASDAQ)]
    public void Parse_valid_file_returns_success_for_known_exchanges(string exchangeStr, Exchange expected)
    {
        var result = _sut.Parse(ValidFile(exchange: exchangeStr));

        result.IsSuccess.Should().BeTrue();
        result.Value.Exchange.Should().Be(expected);
    }

    [Fact]
    public void Parse_extracts_correct_date_and_time()
    {
        var result = _sut.Parse(ValidFile(date: "2026-03-15", time: "14:45:00"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Date.Should().Be(new DateOnly(2026, 3, 15));
        result.Value.Time.Should().Be(new TimeOnly(14, 45, 0));
    }

    [Fact]
    public void Parse_extracts_correct_stock_record_values()
    {
        var result = _sut.Parse(ValidFile(
            dataRows: "AAPL,Apple Inc,182.50,185.00,181.90,184.20,75432100,1.23"));

        result.IsSuccess.Should().BeTrue();
        var record = result.Value.Records.Single();
        record.Symbol.Should().Be("AAPL");
        record.Name.Should().Be("Apple Inc");
        record.Open.Should().Be(182.50m);
        record.High.Should().Be(185.00m);
        record.Low.Should().Be(181.90m);
        record.Close.Should().Be(184.20m);
        record.Volume.Should().Be(75432100L);
        record.ChangePercent.Should().Be(1.23m);
    }

    [Fact]
    public void Parse_handles_multiple_records()
    {
        var content = ValidFile(
            dataRows: "AAPL,Apple,182.50,185.00,181.90,184.20,75432100,1.23\n" +
                      "MSFT,Microsoft,415.10,418.00,413.50,416.80,32100000,-0.45");

        var result = _sut.Parse(content);

        result.IsSuccess.Should().BeTrue();
        result.Value.Records.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_handles_negative_change_percent()
    {
        var result = _sut.Parse(ValidFile(dataRows: "VOD.L,Vodafone,76.42,77.15,75.98,76.83,12345678,-0.15"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Records.Single().ChangePercent.Should().Be(-0.15m);
    }

    // ── Phase 1: content checks ───────────────────────────────────────────

    [Fact]
    public void Parse_empty_string_returns_failure()
    {
        var result = _sut.Parse(string.Empty);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainAny("empty", "whitespace");
    }

    [Fact]
    public void Parse_whitespace_only_returns_failure()
    {
        var result = _sut.Parse("   \n\t  ");
        result.IsFailure.Should().BeTrue();
    }

    // ── Phase 2: structure checks ─────────────────────────────────────────

    [Fact]
    public void Parse_file_without_separator_returns_failure()
    {
        var content = """
            EXCHANGE:FTSE500
            DATE:2026-04-12
            TIME:09:30:00
            SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
            BP.L,BP PLC,450.20,455.10,449.80,453.60,5234567,0.75
            """;

        var result = _sut.Parse(content);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("---");
    }

    [Fact]
    public void Parse_file_with_only_column_header_and_no_data_rows_returns_failure()
    {
        var content = """
            EXCHANGE:FTSE500
            DATE:2026-04-12
            TIME:09:30:00
            ---
            SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
            """;

        var result = _sut.Parse(content);
        result.IsFailure.Should().BeTrue();
    }

    // ── Phase 3: exchange checks ──────────────────────────────────────────

    [Fact]
    public void Parse_missing_exchange_field_returns_failure()
    {
        var content = """
            DATE:2026-04-12
            TIME:09:30:00
            ---
            SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
            BP.L,BP PLC,450.20,455.10,449.80,453.60,5234567,0.75
            """;

        var result = _sut.Parse(content);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("EXCHANGE");
    }

    [Fact]
    public void Parse_unknown_exchange_value_returns_failure()
    {
        var result = _sut.Parse(ValidFile(exchange: "LSE"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("LSE");
    }

    // ── Phase 4: date checks ──────────────────────────────────────────────

    [Fact]
    public void Parse_missing_date_field_returns_failure()
    {
        var content = """
            EXCHANGE:NYSE
            TIME:09:30:00
            ---
            SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
            AAPL,Apple,182.50,185.00,181.90,184.20,75432100,1.23
            """;

        var result = _sut.Parse(content);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("DATE");
    }

    [Theory]
    [InlineData("12/04/2026")]
    [InlineData("2026-4-1")]
    [InlineData("not-a-date")]
    public void Parse_invalid_date_format_returns_failure(string badDate)
    {
        var result = _sut.Parse(ValidFile(date: badDate));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainAny("DATE", "date");
    }

    // ── Phase 5: time checks ──────────────────────────────────────────────

    [Fact]
    public void Parse_missing_time_field_returns_failure()
    {
        var content = """
            EXCHANGE:NYSE
            DATE:2026-04-12
            ---
            SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
            AAPL,Apple,182.50,185.00,181.90,184.20,75432100,1.23
            """;

        var result = _sut.Parse(content);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("TIME");
    }

    [Theory]
    [InlineData("9:30")]
    [InlineData("9am")]
    [InlineData("25:00:00")]
    public void Parse_invalid_time_format_returns_failure(string badTime)
    {
        var result = _sut.Parse(ValidFile(time: badTime));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainAny("TIME", "time");
    }

    // ── Phase 6: column header checks ────────────────────────────────────

    [Fact]
    public void Parse_missing_required_column_returns_failure()
    {
        // Remove VOLUME column
        var result = _sut.Parse(ValidFile(
            columnHeader: "SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,CHANGE_PCT",
            dataRows: "BP.L,BP PLC,450.20,455.10,449.80,453.60,0.75"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("VOLUME");
    }

    // ── Phase 7: record checks ────────────────────────────────────────────

    [Fact]
    public void Parse_record_with_wrong_column_count_returns_failure()
    {
        var result = _sut.Parse(ValidFile(dataRows: "BP.L,BP PLC,450.20,455.10"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainAny("column", "columns");
    }

    [Fact]
    public void Parse_record_with_zero_open_price_returns_failure()
    {
        var result = _sut.Parse(ValidFile(dataRows: "BP.L,BP PLC,0,455.10,449.80,453.60,5234567,0.75"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("OPEN");
    }

    [Fact]
    public void Parse_record_with_non_numeric_price_returns_failure()
    {
        var result = _sut.Parse(ValidFile(dataRows: "BP.L,BP PLC,N/A,455.10,449.80,453.60,5234567,0.75"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("OPEN");
    }

    [Fact]
    public void Parse_record_where_high_is_less_than_low_returns_failure()
    {
        // HIGH=100 < LOW=200 — invalid
        var result = _sut.Parse(ValidFile(dataRows: "BP.L,BP PLC,150.00,100.00,200.00,180.00,5234567,0.75"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainAny("HIGH", "LOW");
    }

    [Fact]
    public void Parse_record_with_negative_volume_returns_failure()
    {
        var result = _sut.Parse(ValidFile(dataRows: "BP.L,BP PLC,450.20,455.10,449.80,453.60,-100,0.75"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("VOLUME");
    }

    [Fact]
    public void Parse_record_with_empty_symbol_returns_failure()
    {
        var result = _sut.Parse(ValidFile(dataRows: ",BP PLC,450.20,455.10,449.80,453.60,5234567,0.75"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("SYMBOL");
    }

    [Fact]
    public void Parse_collects_all_record_errors_not_just_first()
    {
        // Two bad records — both errors should be reported
        var content = ValidFile(
            dataRows: ",Missing Symbol,450.20,455.10,449.80,453.60,5234567,0.75\n" +
                      "BAD.L,Bad Prices,0,0,0,0,5234567,0.75");

        var result = _sut.Parse(content);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("2 record(s)");
    }
}
