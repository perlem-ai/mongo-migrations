using Hdp.Infrastructure.Mongo.Migrations.Migrations.Database;
using Hdp.Versioning;
using MongoDB.Driver;

namespace Hdp.Infrastructure.Mongo.Migrations.Services;

internal class DatabaseMigrationHistoryService
{
    private readonly IMongoDatabase _database;
    private const string MigrationCollectionName = "_migrations";

    public DatabaseMigrationHistoryService(IMongoDatabase database)
    {
        _database = database;
    }

    public Task<DataVersion?> GetLastMigrationVersion() 
        => GetMigrationCollection()
            .Find(m => true)
            .SortByDescending(m => m.Version)
            .Project(m => m.Version)
            .FirstOrDefaultAsync();

    
    public Task SaveAppliedMigration(IDatabaseMigration migration)
    {
        var collection = GetMigrationCollection();
        var historyItem = new DatabaseMigrationHistoryItem(Type: migration.GetType().FullName!, 
            Version: migration.GetVersion(),
            AppliedAt: DateTime.UtcNow);
        
        return collection.InsertOneAsync(historyItem);
    }
    
    private IMongoCollection<DatabaseMigrationHistoryItem> GetMigrationCollection() 
        => _database.GetCollection<DatabaseMigrationHistoryItem>(MigrationCollectionName);


    private record DatabaseMigrationHistoryItem(string Type, DataVersion Version, DateTime AppliedAt);
}

