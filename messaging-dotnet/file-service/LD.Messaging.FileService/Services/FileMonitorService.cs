using LD.Messaging.Adapter;
using LD.Messaging.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LD.Messaging.FileService.Services;

/// <summary>
/// Background service that watches a configured folder for new or changed .txt files.
/// When a file is detected it is parsed via the adapter (using the Checked monad pipeline)
/// and, on success, published to the appropriate Kafka topic.
/// </summary>
public sealed class FileMonitorService(
    ILogger<FileMonitorService> logger,
    IStockFileAdapter adapter,
    StockPublisher publisher,
    IConfiguration configuration)
    : BackgroundService
{
    private readonly string _watchPath = configuration["FileMonitor:WatchPath"] ?? "test-data";
    private readonly string _filter = configuration["FileMonitor:Filter"] ?? "*.txt";
    private FileSystemWatcher? _watcher;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var fullPath = Path.IsPathRooted(_watchPath)
            ? _watchPath
            : Path.Combine(AppContext.BaseDirectory, _watchPath);

        Directory.CreateDirectory(fullPath);

        logger.LogInformation("File monitor started. Watching: {Path} ({Filter})", fullPath, _filter);

        _watcher = new FileSystemWatcher(fullPath, _filter)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnFileEvent;
        _watcher.Changed += OnFileEvent;
        _watcher.Error   += OnWatcherError;

        stoppingToken.Register(DisposeWatcher);
        return Task.CompletedTask;
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        Task.Run(async () =>
        {
            try
            {
                await ProcessFileAsync(e.FullPath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error processing file {Path}", e.FullPath);
            }
        });
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        logger.LogError(e.GetException(), "FileSystemWatcher encountered an error");
    }

    private async Task ProcessFileAsync(string filePath)
    {
        // Brief delay to let the OS finish writing before we read.
        await Task.Delay(300);

        logger.LogInformation("Processing file: {FilePath}", filePath);

        Checked<StockMarketData> result = adapter.ParseFile(filePath)
            .Tap(data => logger.LogInformation(
                "Parsed {Count} records from {Exchange} file (date: {Date})",
                data.Records.Count, data.Exchange, data.Date))
            .TapError(err => logger.LogWarning(
                "Failed to parse {FilePath}: {Error}", filePath, err));

        if (result.IsFailure)
        {
            return;
        }

        await publisher.PublishAsync(result.Value, Path.GetFileName(filePath));
    }

    private void DisposeWatcher()
    {
        _watcher?.Dispose();
        _watcher = null;
    }

    public override void Dispose()
    {
        DisposeWatcher();
        base.Dispose();
    }
}
