using System.Globalization;
using LD.Messaging.Domain;

namespace LD.Messaging.Adapter;

/// <summary>
/// Parses the text-file stock market data format using a railway-oriented pipeline
/// of <see cref="Checked{T}"/> checks. Each phase validates one concern; the first
/// failure short-circuits the chain and surfaces a precise error message.
///
/// Expected file format:
/// <code>
/// EXCHANGE:FTSE500
/// DATE:2026-04-12
/// TIME:09:30:00
/// ---
/// SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
/// BP.L,BP PLC,450.20,455.10,449.80,453.60,5234567,0.75
/// </code>
/// </summary>
public sealed class StockFileParser
{
    private record FileSections(string[] HeaderLines, string[] DataLines);

    // ── Public entry point ────────────────────────────────────────────────

    public Checked<StockMarketData> Parse(string content) =>
        // Phase 1 – content is non-empty
        CheckNotEmpty(content)
            // Phase 2 – file has the '---' separator between header and data
            .Bind(CheckSections)
            // Phases 3-8 – validate header fields then data records
            .Bind(sections =>
                CheckExchange(sections.HeaderLines)
                    .Bind(exchange =>
                        CheckDate(sections.HeaderLines)
                            .Bind(date =>
                                CheckTime(sections.HeaderLines)
                                    .Bind(time =>
                                        CheckColumnHeaders(sections.DataLines)
                                            .Bind(columns =>
                                                ParseRecords(sections.DataLines, columns)
                                                    .Map(records =>
                                                        new StockMarketData(exchange, date, time, records)))))));

    // ── Phase 1: content ─────────────────────────────────────────────────

    private static Checked<string> CheckNotEmpty(string content)
    {
        return string.IsNullOrWhiteSpace(content)
            ? Checked<string>.Fail("File content is empty or contains only whitespace")
            : Checked<string>.Ok(content);
    }

    // ── Phase 2: structure ───────────────────────────────────────────────

    private static Checked<FileSections> CheckSections(string content)
    {
        var lines = content.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

        var separatorIndex = Array.IndexOf(lines, "---");
        if (separatorIndex < 0)
            return Checked<FileSections>.Fail(
                "File is missing the required '---' section separator between header and data");

        var headerLines = lines[..separatorIndex]
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .ToArray();

        var dataLines = lines[(separatorIndex + 1)..]
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .ToArray();

        if (headerLines.Length == 0)
            return Checked<FileSections>.Fail("Header section is empty");

        // dataLines[0] = column-header row, dataLines[1..] = data rows
        if (dataLines.Length < 2)
            return Checked<FileSections>.Fail(
                "Data section must contain at least a column-header row and one record row");

        return Checked<FileSections>.Ok(new FileSections(headerLines, dataLines));
    }

    // ── Phase 3: exchange ────────────────────────────────────────────────

    private static Checked<Exchange> CheckExchange(string[] headerLines)
    {
        var line = headerLines.FirstOrDefault(l => l.StartsWith("EXCHANGE:", StringComparison.OrdinalIgnoreCase));
        if (line is null)
            return Checked<Exchange>.Fail("Header is missing required field: EXCHANGE");

        var value = line["EXCHANGE:".Length..].Trim();
        if (!Enum.TryParse<Exchange>(value, ignoreCase: true, out var exchange) || exchange == Exchange.Unknown)
            return Checked<Exchange>.Fail(
                $"Unknown exchange '{value}'. Supported values: FTSE500, NYSE, NASDAQ");

        return Checked<Exchange>.Ok(exchange);
    }

    // ── Phase 4: date ────────────────────────────────────────────────────

    private static Checked<DateOnly> CheckDate(string[] headerLines)
    {
        var line = headerLines.FirstOrDefault(l => l.StartsWith("DATE:", StringComparison.OrdinalIgnoreCase));
        if (line is null)
            return Checked<DateOnly>.Fail("Header is missing required field: DATE");

        var value = line["DATE:".Length..].Trim();
        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var date)
            ? Checked<DateOnly>.Ok(date)
            : Checked<DateOnly>.Fail($"Invalid DATE format '{value}'. Expected: yyyy-MM-dd");
    }

    // ── Phase 5: time ────────────────────────────────────────────────────

    private static Checked<TimeOnly> CheckTime(string[] headerLines)
    {
        var line = headerLines.FirstOrDefault(l => l.StartsWith("TIME:", StringComparison.OrdinalIgnoreCase));
        if (line is null)
            return Checked<TimeOnly>.Fail("Header is missing required field: TIME");

        var value = line["TIME:".Length..].Trim();
        return TimeOnly.TryParseExact(value, "HH:mm:ss", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var time)
            ? Checked<TimeOnly>.Ok(time)
            : Checked<TimeOnly>.Fail($"Invalid TIME format '{value}'. Expected: HH:mm:ss");
    }

    // ── Phase 6: column headers ──────────────────────────────────────────

    private static readonly string[] RequiredColumns =
        ["SYMBOL", "NAME", "OPEN", "HIGH", "LOW", "CLOSE", "VOLUME", "CHANGE_PCT"];

    private static Checked<string[]> CheckColumnHeaders(string[] dataLines)
    {
        var columns = dataLines[0]
            .Split(',')
            .Select(c => c.Trim().ToUpperInvariant())
            .ToArray();

        var missing = RequiredColumns.Except(columns).ToArray();
        if (missing.Length > 0)
            return Checked<string[]>.Fail(
                $"Data section is missing required column(s): {string.Join(", ", missing)}");

        return Checked<string[]>.Ok(columns);
    }

    // ── Phase 7: record parsing ──────────────────────────────────────────

    private static Checked<IReadOnlyList<StockRecord>> ParseRecords(string[] dataLines, string[] columns)
    {
        var records = new List<StockRecord>();
        var errors = new List<string>();

        // dataLines[0] is the column-header row; start from index 1
        foreach (var (line, lineNumber) in dataLines.Skip(1).Select((l, i) => (l, i + 2)))
        {
            var result = ParseSingleRecord(line, columns, lineNumber);
            if (result.IsSuccess)
                records.Add(result.Value);
            else
                errors.Add(result.Error);
        }

        if (errors.Count > 0)
            return Checked<IReadOnlyList<StockRecord>>.Fail(
                $"{errors.Count} record(s) failed validation:\n  " + string.Join("\n  ", errors));

        if (records.Count == 0)
            return Checked<IReadOnlyList<StockRecord>>.Fail("No records found in data section");

        return Checked<IReadOnlyList<StockRecord>>.Ok(records.AsReadOnly());
    }

    private static Checked<StockRecord> ParseSingleRecord(string line, string[] columns, int lineNumber)
    {
        var fields = line.Split(',');
        if (fields.Length != columns.Length)
            return Checked<StockRecord>.Fail(
                $"Line {lineNumber}: expected {columns.Length} columns but found {fields.Length}");

        int Col(string name) => Array.IndexOf(columns, name);

        var symbol = fields[Col("SYMBOL")].Trim();
        if (string.IsNullOrWhiteSpace(symbol))
            return Checked<StockRecord>.Fail($"Line {lineNumber}: SYMBOL is empty");

        var name = fields[Col("NAME")].Trim();

        if (!TryParsePositiveDecimal(fields[Col("OPEN")], out var open))
            return Checked<StockRecord>.Fail($"Line {lineNumber} [{symbol}]: OPEN must be a positive number");

        if (!TryParsePositiveDecimal(fields[Col("HIGH")], out var high))
            return Checked<StockRecord>.Fail($"Line {lineNumber} [{symbol}]: HIGH must be a positive number");

        if (!TryParsePositiveDecimal(fields[Col("LOW")], out var low))
            return Checked<StockRecord>.Fail($"Line {lineNumber} [{symbol}]: LOW must be a positive number");

        if (!TryParsePositiveDecimal(fields[Col("CLOSE")], out var close))
            return Checked<StockRecord>.Fail($"Line {lineNumber} [{symbol}]: CLOSE must be a positive number");

        if (high < low)
            return Checked<StockRecord>.Fail(
                $"Line {lineNumber} [{symbol}]: HIGH ({high}) cannot be less than LOW ({low})");

        if (!long.TryParse(fields[Col("VOLUME")].Trim(), out var volume) || volume < 0)
            return Checked<StockRecord>.Fail($"Line {lineNumber} [{symbol}]: VOLUME must be a non-negative integer");

        if (!decimal.TryParse(fields[Col("CHANGE_PCT")].Trim(), NumberStyles.Number,
                CultureInfo.InvariantCulture, out var changePct))
            return Checked<StockRecord>.Fail($"Line {lineNumber} [{symbol}]: CHANGE_PCT is not a valid decimal");

        return Checked<StockRecord>.Ok(new StockRecord(symbol, name, open, high, low, close, volume, changePct));
    }

    private static bool TryParsePositiveDecimal(string raw, out decimal value)
    {
        if (decimal.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            return value > 0;
        value = 0;
        return false;
    }
}
