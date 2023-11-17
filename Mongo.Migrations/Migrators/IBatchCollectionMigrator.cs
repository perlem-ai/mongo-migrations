namespace Hdp.Infrastructure.Mongo.Migrations.Migrators;

internal interface IBatchCollectionMigrator
{
    Task Migrate(CancellationToken cancellationToken);
}