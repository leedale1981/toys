using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LD.Messaging.Adapter;
using LD.Messaging.Domain;
using LD.Messaging.StockGenerator;

namespace LD.Messaging.FileService.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks the file-service adapter pipeline:
/// <list type="bullet">
///   <item><see cref="ParseContent_Small"/>  – 10-record payload, no I/O</item>
///   <item><see cref="ParseContent_Medium"/> – 50-record payload, no I/O</item>
///   <item><see cref="ParseContent_Large"/>  – 100-record payload (theoretical max), no I/O</item>
///   <item><see cref="ParseFile_Small"/>     – 10-record payload read from disk</item>
///   <item><see cref="ParseFile_Large"/>     – 100-record payload read from disk</item>
/// </list>
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class AdapterBenchmarks
{
    private StockFileAdapter _adapter = null!;
    private StockFileGenerator _generator = null!;

    private string _smallContent = null!;
    private string _mediumContent = null!;
    private string _largeContent = null!;

    private string _smallFilePath = null!;
    private string _largeFilePath = null!;

    [GlobalSetup]
    public void Setup()
    {
        _adapter = new StockFileAdapter();
        _generator = new StockFileGenerator();

        _smallContent = _generator.Generate(Exchange.FTSE500, recordCount: 10);
        _mediumContent = BuildLargeContent(Exchange.NYSE, 50);
        _largeContent = BuildLargeContent(Exchange.NASDAQ, 100);

        _smallFilePath = WriteTempFile(_smallContent);
        _largeFilePath = WriteTempFile(_largeContent);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        DeleteTempFile(_smallFilePath);
        DeleteTempFile(_largeFilePath);
    }

    // ── ParseContent ─────────────────────────────────────────────────────────

    [Benchmark(Description = "ParseContent – 10 records")]
    public void ParseContent_Small()
    {
        _adapter.ParseContent(_smallContent);
    }

    [Benchmark(Description = "ParseContent – 50 records")]
    public void ParseContent_Medium()
    {
        _adapter.ParseContent(_mediumContent);
    }

    [Benchmark(Description = "ParseContent – 100 records")]
    public void ParseContent_Large()
    {
        _adapter.ParseContent(_largeContent);
    }

    // ── ParseFile ─────────────────────────────────────────────────────────────

    [Benchmark(Description = "ParseFile – 10 records (disk I/O)")]
    public void ParseFile_Small()
    {
        _adapter.ParseFile(_smallFilePath);
    }

    [Benchmark(Description = "ParseFile – 100 records (disk I/O)")]
    public void ParseFile_Large()
    {
        _adapter.ParseFile(_largeFilePath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// The generator only has 10 symbols per exchange, so for larger payloads we
    /// repeat the generation and merge the record lines under a single header.
    /// </summary>
    private string BuildLargeContent(Exchange exchange, int targetRows)
    {
        var lines = new List<string>();
        string? header = null;

        while (lines.Count < targetRows)
        {
            var raw = _generator.Generate(exchange, recordCount: 10);
            var allLines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var separatorIndex = Array.IndexOf(allLines, "---");

            if (header is null)
            {
                // Keep the exchange/date/time header and the column-header row
                header = string.Join('\n', allLines[..(separatorIndex + 2)]);
            }

            // Skip the file header (before "---") and the column-header row
            foreach (var line in allLines[(separatorIndex + 2)..])
            {
                lines.Add(line);
            }
        }

        var records = string.Join('\n', lines.Take(targetRows));
        return header + '\n' + records + '\n';
    }

    private static string WriteTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"bench_{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, content);
        return path;
    }

    private static void DeleteTempFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
