using LD.Messaging.Infrastructure.Persistence.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LD.Messaging.Infrastructure.Persistence;

/// <summary>
/// Extension methods for registering persistence services with the dependency injection container.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Adds stock records persistence services to the service collection.
    /// 
    /// Registers:
    /// - StockRecordsDbContext configured for PostgreSQL
    /// - SaveStockRecordsCommandHandler for handling persistence commands
    /// 
    /// Database Connection:
    /// The PostgreSQL connection string is read from configuration key "ConnectionStrings:StockRecordsDb"
    /// </summary>
    public static IServiceCollection AddStockRecordsPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException(
                "PostgreSQL connection string cannot be null or empty",
                nameof(connectionString));

        // Register DbContext with PostgreSQL provider
        services.AddDbContext<StockRecordsDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    // Enable automatic query parameters to prevent any possibility of SQL injection
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
        });

        // Register command handler
        services.AddScoped<SaveStockRecordsCommandHandler>();

        return services;
    }
}

