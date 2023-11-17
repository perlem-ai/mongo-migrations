using Hdp.Infrastructure.Mongo.Migrations.Migrations.Collection;
using Hdp.Infrastructure.Mongo.Migrations.Registries;
using Hdp.Infrastructure.Mongo.Migrations.Services;
using Hdp.Versioning;
using MongoDB.Driver;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrators;

internal class BatchCollectionMigrator<TDocument> : IBatchCollectionMigrator
    where TDocument : IVersioned
{
    private readonly BatchCollectionMigrationSequence<TDocument> _migrationSequence;
    private readonly CollectionLocator<TDocument> _collectionLocator;
    private readonly GranularCollectionMigrator<TDocument> _granularCollectionMigrator;

    public BatchCollectionMigrator(BatchCollectionMigrationSequence<TDocument> migrationSequence, 
        CollectionLocator<TDocument> collectionLocator,
        GranularCollectionMigrator<TDocument> granularCollectionMigrator)
    {
        _migrationSequence = migrationSequence;
        _collectionLocator = collectionLocator;
        _granularCollectionMigrator = granularCollectionMigrator;
    }

    public async Task Migrate(CancellationToken cancellationToken)
    {
        var collection = _collectionLocator.Get();

        foreach (var batchMigration in _migrationSequence)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await HasPreviousVersionDocuments(collection, batchMigration, cancellationToken))
            {
                await _granularCollectionMigrator.Migrate(batchMigration.VersionFilter, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (await HasDocumentsToUpdate(collection, batchMigration, cancellationToken))
            {
                await batchMigration.Up(cancellationToken);
            }
        }

        static async Task<bool> HasPreviousVersionDocuments(IMongoCollection<TDocument> collection, IBatchCollectionMigration migration,
            CancellationToken cancellationToken)
        {
            var itemsToUpdate = await collection
                .CountDocumentsAsync(document => document.Version < migration.VersionFilter,
                    cancellationToken: cancellationToken);

            return itemsToUpdate > 0;
        }
        
        static async Task<bool> HasDocumentsToUpdate(IMongoCollection<TDocument> collection, IBatchCollectionMigration migration,
            CancellationToken cancellationToken)
        {
            var itemsToUpdate = await collection
                .CountDocumentsAsync(document => document.Version == migration.VersionFilter,
                    cancellationToken: cancellationToken);

            return itemsToUpdate > 0;
        }
    }
}