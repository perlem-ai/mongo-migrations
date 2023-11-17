using Hdp.Infrastructure.Mongo.Migrations.Migrations.Database;
using Hdp.Infrastructure.Mongo.Migrations.Registries;
using Hdp.Infrastructure.Mongo.Migrations.Services;
using Hdp.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrators;

internal class DatabaseMigrator
{
    private readonly IMongoDatabase _database;
    private readonly DatabaseMigrationsSequence _migrationsSequence;
    private readonly DatabaseMigrationHistoryService _databaseMigrationHistoryService;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(IMongoDatabase database, DatabaseMigrationsSequence migrationsSequence,
        DatabaseMigrationHistoryService databaseMigrationHistoryService, ILoggerFactory? loggerFactory)
    {
        _database = database;
        _migrationsSequence = migrationsSequence;
        _databaseMigrationHistoryService = databaseMigrationHistoryService;
        _logger = loggerFactory?.CreateLogger<DatabaseMigrator>() ?? NullLogger<DatabaseMigrator>.Instance;
    }
    
    public async Task Migrate(CancellationToken cancellationToken)
    {
        var databaseVersion = await _databaseMigrationHistoryService.GetLastMigrationVersion() ?? DataVersion.Empty();
        _logger.LogInformation("Database version: {0}", databaseVersion);
        
        var migrationsToApply = _migrationsSequence
            .Where(m => m.GetVersion() > databaseVersion)
            .ToList();
        
        _logger.LogInformation("Found {0} database-scoped migrations to apply", migrationsToApply.Count);

        foreach (var migration in migrationsToApply)
            await Apply(migration);

        
        async Task Apply(IDatabaseMigration migration)
        {
            try
            {
                _logger.LogInformation("Applying migration {0}, version {1}", migration.GetType().FullName, migration.GetVersion());
                await migration.Up(cancellationToken);
                await _databaseMigrationHistoryService.SaveAppliedMigration(migration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in applying database migration: {0}", migration.GetType().FullName);
                throw;
            }
        }
    }
}