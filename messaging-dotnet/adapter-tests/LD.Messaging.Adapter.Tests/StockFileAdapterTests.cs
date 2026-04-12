using FluentAssertions;
using LD.Messaging.Domain;
using Xunit;

namespace LD.Messaging.Adapter.Tests;

/// <summary>
/// Tests for <see cref="StockFileAdapter"/> focusing on file-system concerns:
/// missing files, I/O errors, and end-to-end parsing from actual temp files.
/// The parser logic is covered in <see cref="StockFileParserTests"/>.
/// </summary>
public class StockFileAdapterTests : IDisposable
{
    private readonly StockFileAdapter _sut = new();
    private readonly List<string> _tempFiles = [];

    // ── ParseFile: file system checks ────────────────────────────────────

    [Fact]
    public void ParseFile_with_null_or_empty_path_returns_failure()
    {
        _sut.ParseFile(string.Empty).IsFailure.Should().BeTrue();
        _sut.ParseFile("   ").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ParseFile_with_nonexistent_path_returns_failure()
    {
        var result = _sut.ParseFile("D:/does/not/exist/file.txt");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void ParseFile_reads_and_parses_a_valid_temp_file()
    {
        var path = WriteTempFile("""
            EXCHANGE:NYSE
            DATE:2026-04-12
            TIME:10:00:00
            ---
            SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
            AAPL,Apple Inc,182.50,185.00,181.90,184.20,75432100,1.23
            MSFT,Microsoft,415.10,418.00,413.50,416.80,32100000,-0.45
            """);

        var result = _sut.ParseFile(path);

        result.IsSuccess.Should().BeTrue();
        result.Value.Exchange.Should().Be(Exchange.NYSE);
        result.Value.Records.Should().HaveCount(2);
    }

    [Fact]
    public void ParseFile_returns_failure_for_malformed_temp_file()
    {
        var path = WriteTempFile("""
            EXCHANGE:UNKNOWN_EXCHANGE
            DATE:2026-04-12
            TIME:10:00:00
            ---
            SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
            AAPL,Apple Inc,182.50,185.00,181.90,184.20,75432100,1.23
            """);

        var result = _sut.ParseFile(path);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("UNKNOWN_EXCHANGE");
    }

    // ── ParseContent: round-trip ──────────────────────────────────────────

    [Fact]
    public void ParseContent_delegates_to_the_parser_and_returns_success()
    {
        var result = _sut.ParseContent("""
            EXCHANGE:NASDAQ
            DATE:2026-04-12
            TIME:09:30:00
            ---
            SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
            TSLA,Tesla Inc,173.50,176.20,172.80,175.10,50123000,0.92
            """);

        result.IsSuccess.Should().BeTrue();
        result.Value.Exchange.Should().Be(Exchange.NASDAQ);
        result.Value.Records.Single().Symbol.Should().Be("TSLA");
    }

    [Fact]
    public void ParseContent_returns_failure_for_empty_content()
    {
        _sut.ParseContent(string.Empty).IsFailure.Should().BeTrue();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private string WriteTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"stock_test_{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var f in _tempFiles.Where(File.Exists))
            File.Delete(f);
    }
}
