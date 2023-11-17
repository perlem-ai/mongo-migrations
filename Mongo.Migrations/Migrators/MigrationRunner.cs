using Hdp.Infrastructure.Mongo.Migrations.Extensions;
using Hdp.Infrastructure.Mongo.Migrations.Registries;
using Hdp.Infrastructure.Mongo.Migrations.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrators;

public class MigrationRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<MigrationRunner> _logger;
    private readonly int _parallelMigrationsCount;

    public MigrationRunner(IOptionsMonitor<MigrationWorkerSettings> settingsMonitor,
        IServiceProvider serviceProvider, ILoggerFactory? loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _parallelMigrationsCount = settingsMonitor.CurrentValue.CollectionsInParallel ?? 1;
        _semaphore = new SemaphoreSlim(_parallelMigrationsCount);
        _logger = loggerFactory?.CreateLogger<MigrationRunner>() ?? NullLogger<MigrationRunner>.Instance;
    }
    
    public async Task Migrate(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database-scoped migrations");
        var databaseMigrator = _serviceProvider.GetRequiredService<DatabaseMigrator>();
        await databaseMigrator.Migrate(cancellationToken);
        _logger.LogInformation("Finished database-scoped migration");
        
        _logger.LogInformation("Starting collection-scoped migrations");
        var migrationTasks = DocumentMigrationsRegistry.DocumentsWithMigrations
            .Union(BatchCollectionMigrationsRegistry.DocumentsWithMigrations)
            .Select(RunMigration)
            .ToArray();
        
        _logger.LogInformation($"Executing {migrationTasks.Length} migration tasks with {_parallelMigrationsCount} migrations in parallel");

        await Task.WhenAll(migrationTasks);
        _logger.LogInformation("Finished collection-scoped migrations");

        async Task RunMigration(Type docType)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                var batchMigrator = _serviceProvider
                    .ResolveGeneric<IBatchCollectionMigrator>(typeof(BatchCollectionMigrator<>), docType);

                await batchMigrator.Migrate(cancellationToken);

                var granularMigrator = _serviceProvider
                    .ResolveGeneric<IGranularCollectionMigration>(typeof(GranularCollectionMigrator<>), docType);
                  
                await granularMigrator.UpToLatest(cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}