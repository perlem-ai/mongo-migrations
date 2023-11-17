namespace Hdp.Infrastructure.Mongo.Migrations.Migrators;

internal interface IGranularCollectionMigration
{
    Task UpToLatest(CancellationToken cancellationToken);
}

