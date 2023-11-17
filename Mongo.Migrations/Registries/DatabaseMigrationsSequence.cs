using System.Collections;
using Hdp.Infrastructure.Mongo.Migrations.Migrations.Database;

namespace Hdp.Infrastructure.Mongo.Migrations.Registries;

internal class DatabaseMigrationsSequence : IEnumerable<IDatabaseMigration>
{
    public DatabaseMigrationsSequence(IEnumerable<IDatabaseMigration> migrations)
    {
        _databaseMigrations = migrations
            .OrderBy(m => m.GetVersion())
            .DistinctByType()
            .ToArray();
    }

    private readonly IReadOnlyCollection<IDatabaseMigration> _databaseMigrations;
    
    public IEnumerator<IDatabaseMigration> GetEnumerator() 
        => _databaseMigrations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() 
        => GetEnumerator();
}