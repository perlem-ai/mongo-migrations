using Hdp.Versioning;
using MongoDB.Driver;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrations.Database;

public abstract class DatabaseMigrationBase : IDatabaseMigration
{
    protected readonly IMongoDatabase Database;

    public DatabaseMigrationBase(IMongoDatabase database)
    {
        Database = database;
    }
    
    public abstract DataVersion GetVersion();
    public abstract Task Up(CancellationToken cancellationToken);
    public abstract Task Down(CancellationToken cancellationToken);
}