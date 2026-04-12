using LD.Messaging.Domain;

namespace LD.Messaging.Adapter;

/// <summary>
/// Default implementation of <see cref="IStockFileAdapter"/>. Handles file I/O
/// errors before delegating to <see cref="StockFileParser"/> for content validation.
/// </summary>
public sealed class StockFileAdapter : IStockFileAdapter
{
    private readonly StockFileParser _parser = new();

    public Checked<StockMarketData> ParseFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Checked<StockMarketData>.Fail("File path must not be empty");

        if (!File.Exists(filePath))
            return Checked<StockMarketData>.Fail($"File not found: {filePath}");

        try
        {
            var content = File.ReadAllText(filePath);
            return _parser.Parse(content);
        }
        catch (IOException ex)
        {
            return Checked<StockMarketData>.Fail($"Failed to read '{filePath}': {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Checked<StockMarketData>.Fail($"Access denied to '{filePath}': {ex.Message}");
        }
    }

    public Checked<StockMarketData> ParseContent(string content)
        => _parser.Parse(content);
}
