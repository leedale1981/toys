using LD.Messaging.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LD.Messaging.StockGenerator.Services;

/// <summary>
/// Background service that writes synthetic stock files on a fixed interval.
/// Files are written to the configured output directory in the format expected
/// by <c>FileMonitorService</c> in the file-service project.
/// </summary>
public sealed class FileGeneratorService : BackgroundService
{
    private static readonly Exchange[] Exchanges =
        [Exchange.FTSE500, Exchange.NYSE, Exchange.NASDAQ];

    private readonly ILogger<FileGeneratorService> _logger;
    private readonly StockFileGenerator _generator;
    private readonly string _outputPath;
    private readonly TimeSpan _interval;
    private readonly int _recordCount;

    public FileGeneratorService(
        ILogger<FileGeneratorService> logger,
        StockFileGenerator generator,
        IConfiguration configuration)
    {
        _logger = logger;
        _generator = generator;
        _outputPath = configuration["Generator:OutputPath"] ?? "generated-data";
        _interval = TimeSpan.FromMilliseconds(
            double.Parse(configuration["Generator:IntervalMs"] ?? "1000"));
        _recordCount = int.Parse(configuration["Generator:RecordCount"] ?? "10");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var fullPath = Path.IsPathRooted(_outputPath)
            ? _outputPath
            : Path.Combine(AppContext.BaseDirectory, _outputPath);

        Directory.CreateDirectory(fullPath);

        _logger.LogInformation(
            "Stock file generator started. Output: {Path}, interval: {Interval}ms, records: {Records}",
            fullPath, _interval.TotalMilliseconds, _recordCount);

        var exchangeIndex = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var exchange = Exchanges[exchangeIndex % Exchanges.Length];
            exchangeIndex++;

            try
            {
                WriteFile(fullPath, exchange);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write generated file for {Exchange}", exchange);
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private void WriteFile(string directory, Exchange exchange)
    {
        var content = _generator.Generate(exchange, _recordCount);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        var fileName = $"{exchange.ToString().ToLowerInvariant()}_{timestamp}.txt";
        var filePath = Path.Combine(directory, fileName);

        File.WriteAllText(filePath, content);

        _logger.LogInformation("Generated: {FileName}", fileName);
    }
}
