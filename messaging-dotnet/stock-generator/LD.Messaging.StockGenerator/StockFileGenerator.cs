using System.Globalization;
using System.Text;
using LD.Messaging.Domain;

namespace LD.Messaging.StockGenerator;

/// <summary>
/// Generates synthetic stock-market data files in the format expected by
/// <c>StockFileParser</c>. Each call to <see cref="Generate"/> produces a
/// complete, valid file as a string.
/// </summary>
public sealed class StockFileGenerator
{
    private static readonly Random Rng = Random.Shared;

    private static readonly IReadOnlyDictionary<Exchange, (string Symbol, string Name, decimal BasePrice)[]> SymbolMap =
        new Dictionary<Exchange, (string, string, decimal)[]>
        {
            [Exchange.FTSE500] =
            [
                ("BP.L",   "BP PLC",             450m),
                ("HSBA.L", "HSBC Holdings",       625m),
                ("VOD.L",  "Vodafone Group",        76m),
                ("SHEL.L", "Shell PLC",           2748m),
                ("ULVR.L", "Unilever PLC",        3890m),
                ("AZN.L",  "AstraZeneca",        11450m),
                ("LLOY.L", "Lloyds Banking",        53m),
                ("BA.L",   "BAE Systems",         1342m),
                ("GSK.L",  "GSK PLC",             1580m),
                ("RIO.L",  "Rio Tinto",           5120m),
            ],
            [Exchange.NYSE] =
            [
                ("JPM",  "JPMorgan Chase",       195m),
                ("BAC",  "Bank of America",       37m),
                ("WFC",  "Wells Fargo",            57m),
                ("GS",   "Goldman Sachs",         448m),
                ("MS",   "Morgan Stanley",        101m),
                ("C",    "Citigroup",              63m),
                ("USB",  "U.S. Bancorp",           44m),
                ("PNC",  "PNC Financial",         157m),
                ("TFC",  "Truist Financial",       39m),
                ("COF",  "Capital One",           142m),
            ],
            [Exchange.NASDAQ] =
            [
                ("AAPL",  "Apple Inc.",           172m),
                ("MSFT",  "Microsoft Corp.",      415m),
                ("AMZN",  "Amazon.com",           181m),
                ("GOOGL", "Alphabet Inc.",        174m),
                ("META",  "Meta Platforms",       510m),
                ("NVDA",  "NVIDIA Corp.",         877m),
                ("TSLA",  "Tesla Inc.",           178m),
                ("AMD",   "Advanced Micro Devices", 165m),
                ("INTC",  "Intel Corp.",           30m),
                ("QCOM",  "Qualcomm Inc.",        168m),
            ],
        };

    /// <summary>
    /// Generates a complete stock file for the given exchange with <paramref name="recordCount"/>
    /// randomly selected symbols. If <paramref name="recordCount"/> exceeds the available
    /// symbols the full symbol list is used.
    /// </summary>
    public string Generate(Exchange exchange, int recordCount = 10)
    {
        var now = DateTime.UtcNow;
        var symbols = SymbolMap[exchange];
        var selected = symbols
            .OrderBy(_ => Rng.Next())
            .Take(Math.Min(recordCount, symbols.Length))
            .ToArray();

        var sb = new StringBuilder();
        sb.AppendLine($"EXCHANGE:{exchange}");
        sb.AppendLine($"DATE:{now:yyyy-MM-dd}");
        sb.AppendLine($"TIME:{now:HH:mm:ss}");
        sb.AppendLine("---");
        sb.AppendLine("SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT");

        foreach (var (symbol, name, basePrice) in selected)
        {
            var open = Jitter(basePrice, 0.01m);
            var close = Jitter(basePrice, 0.01m);
            var high = Math.Max(open, close) + Jitter(basePrice * 0.005m, 0.5m);
            var low = Math.Min(open, close) - Jitter(basePrice * 0.005m, 0.5m);
            var volume = Rng.NextInt64(500_000, 50_000_000);
            var changePct = Math.Round((close - open) / open * 100m, 2);

            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2:F2},{3:F2},{4:F2},{5:F2},{6},{7:F2}",
                symbol, name, open, high, low, close, volume, changePct));
        }

        return sb.ToString();
    }

    private static decimal Jitter(decimal value, decimal maxFraction)
    {
        var delta = value * maxFraction * (decimal)(Rng.NextDouble() * 2 - 1);
        return Math.Max(0.01m, Math.Round(value + delta, 2));
    }
}
