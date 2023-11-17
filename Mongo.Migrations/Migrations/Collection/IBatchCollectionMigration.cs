using Hdp.Versioning;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrations.Collection;

internal interface IBatchCollectionMigration<TDocument> : IBatchCollectionMigration
    where TDocument : IVersioned
{
}

internal interface IBatchCollectionMigration
{
    Task Up(CancellationToken cancellationToken);
    Task Down(CancellationToken cancellationToken);
    DataVersion Version { get; }
    DataVersion VersionFilter { get; }
}