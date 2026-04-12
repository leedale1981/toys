using LD.Messaging.Domain;

namespace LD.Messaging.Adapter;

/// <summary>
/// Parses stock market data files into <see cref="StockMarketData"/>.
/// Each parsing phase is validated through the <see cref="Checked{T}"/> monad so
/// the first failing phase short-circuits with a descriptive error.
/// </summary>
public interface IStockFileAdapter
{
    /// <summary>Reads a file from disk and parses it.</summary>
    Checked<StockMarketData> ParseFile(string filePath);

    /// <summary>Parses already-loaded text content (useful for testing).</summary>
    Checked<StockMarketData> ParseContent(string content);
}
