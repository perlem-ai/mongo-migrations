using System.Collections;
using Hdp.Infrastructure.Mongo.Migrations.Migrations.Collection;
using Hdp.Versioning;

namespace Hdp.Infrastructure.Mongo.Migrations.Registries;

internal class BatchCollectionMigrationSequence<TDocument>: IEnumerable<IBatchCollectionMigration<TDocument>>
    where TDocument : IVersioned
{
    private readonly IReadOnlyCollection<IBatchCollectionMigration<TDocument>> _migrations;

    public BatchCollectionMigrationSequence(IEnumerable<IBatchCollectionMigration<TDocument>> migrations)
    {
        _migrations = migrations
            .OrderBy(m => m.Version)
            .DistinctByType()
            .ToArray();
    }

    public IEnumerator<IBatchCollectionMigration<TDocument>> GetEnumerator() 
        => _migrations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() 
        => GetEnumerator();
}