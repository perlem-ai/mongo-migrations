using Hdp.Infrastructure.Mongo.Migrations.Constants;
using Hdp.Infrastructure.Mongo.Migrations.Contracts.Interfaces;
using Hdp.Infrastructure.Mongo.Migrations.Contracts.Models;
using Hdp.Infrastructure.Mongo.Migrations.Extensions;
using Hdp.Infrastructure.Mongo.Migrations.Services;
using Hdp.Infrastructure.Mongo.Migrations.Settings;
using Hdp.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrators;

internal class GranularCollectionMigrator<TDocument> : IGranularCollectionMigration
    where TDocument: IVersioned
{
    private readonly SingleDocumentMigrator<TDocument> _singleDocumentMigrator;
    private readonly IMongoDatabase _database;
    private readonly ICollectionNameResolver _collectionNameResolver;
    private readonly DocumentCurrentVersionProvider<TDocument> _versionProvider;
    private readonly IOptionsMonitor<DefaultCollectionMigrationSettings> _settings;
    private readonly IDocumentSettingsProvider _documentSettingsProvider;
    private readonly ILogger<GranularCollectionMigrator<TDocument>> _logger;

    private const int DefaultBatchSize = 200;

    public GranularCollectionMigrator(SingleDocumentMigrator<TDocument> singleDocumentMigrator,
        IMongoDatabase database, ICollectionNameResolver collectionNameResolver,
        DocumentCurrentVersionProvider<TDocument> versionProvider,
        IOptionsMonitor<DefaultCollectionMigrationSettings> settings,
        IDocumentSettingsProvider documentSettingsProvider,
        ILoggerFactory? loggerFactory)
    {
        _singleDocumentMigrator = singleDocumentMigrator;
        _database = database;
        _collectionNameResolver = collectionNameResolver;
        _versionProvider = versionProvider;
        _settings = settings;
        _documentSettingsProvider = documentSettingsProvider;
        _logger = loggerFactory?.CreateLogger<GranularCollectionMigrator<TDocument>>()
                  ?? NullLogger<GranularCollectionMigrator<TDocument>>.Instance;
    }

    public Task UpToLatest(CancellationToken cancellationToken)
    {
        var currentVersion = _versionProvider.Get();
        _logger.LogInformation($"Attempt to migrate collection for '{typeof(TDocument).Name}', current version: {currentVersion}");
        if (currentVersion is null)
        {
            _logger.LogInformation("Current version is null, migration skipped");
            return Task.CompletedTask;
        }

        return Migrate(currentVersion, cancellationToken);
    }

    public async Task Migrate(DataVersion toVersion, CancellationToken cancellationToken)
    {
        var collectionName = _collectionNameResolver.GetName(typeof(TDocument));
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var batchSize = _settings.CurrentValue.BatchSize ?? DefaultBatchSize;

        _logger.LogInformation($"Starting migration for collection '{collectionName}', batch size: {batchSize}. Version: {toVersion}");
        var documentSettings = _documentSettingsProvider.Get<TDocument>() ?? DocumentSettings.Default;

        long migratedDocumentsTotal = 0;
        long notMigratedDocumentsTotal;
        do
        {
            notMigratedDocumentsTotal = 0;
            var migratingDocuments = await GetMigratingDocuments(toVersion, collection, batchSize, cancellationToken);
            await foreach (var batch in migratingDocuments.GetAsyncEnumerator())
            {
                var writeOperations = GetMigrationOperations(batch, documentSettings, cancellationToken);
                if (writeOperations.Count == 0)
                    continue;

                var writeResult = await collection.BulkWriteAsync(requests: writeOperations, cancellationToken: cancellationToken);

                migratedDocumentsTotal += writeResult.ModifiedCount;
                _logger.LogInformation($"Migrated {migratedDocumentsTotal} documents");
                
                if (writeResult.ModifiedCount != writeOperations.Count)
                {
                    var notMigratedDocuments = (writeOperations.Count - writeResult.ModifiedCount);
                    notMigratedDocumentsTotal += notMigratedDocuments;
                    _logger.LogWarning($"{notMigratedDocuments} documents were not migrated due to concurrency issues. Migration will be retried for unmodified documents");
                }
            }

            _logger.LogInformation($"Total migrated {migratedDocumentsTotal} documents");
        } 
        while (notMigratedDocumentsTotal != 0);
    }
    
    private IList<WriteModel<BsonDocument>> GetMigrationOperations(IEnumerable<BsonDocument> batch, DocumentSettings settings, CancellationToken cancellationToken)
    {
        var writeOperations = new List<WriteModel<BsonDocument>>();
        foreach (var document in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                // Apply migrations
                var migrated = _singleDocumentMigrator.Migrate(document.ToBsonDocument());

                // Get id filter
                var filter = Builders<BsonDocument>.Filter
                    .Where(bsonDoc => bsonDoc[settings.BsonIdPropertyName] == migrated[settings.BsonIdPropertyName]);

                // Apply optimistic concurrency filters
                if (settings.OptimisticConcurrencyStrategy is { } strategy)
                {
                    var currentStampValue = migrated[strategy.BsonPropertyName];
                    filter = Builders<BsonDocument>.Filter
                        .And(filter, new ExpressionFilterDefinition<BsonDocument>(bsonDocument =>
                            bsonDocument[strategy.BsonPropertyName] == currentStampValue));

                    migrated.Set(strategy.BsonPropertyName, strategy.StampFactory().ToString());
                }

                var replaceModel = new ReplaceOneModel<BsonDocument>(filter, migrated) { IsUpsert = false };

                writeOperations.Add(replaceModel);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error while migrating document '{document[settings.BsonIdPropertyName]}'");
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        return writeOperations;
    }

    private static Task<IAsyncCursor<BsonDocument>> GetMigratingDocuments(DataVersion toVersion, IMongoCollection<BsonDocument> collection, int batchSize, CancellationToken cancellationToken)
    {
        // TODO: разобраться, почему кастомный сериализатор не вызывается для FindAsync
        const string major = "major";
        const string minor = "minor";
        const string revision = "revision";
        
        return collection
            .FindAsync(
                filter: doc =>
                    doc[PropertyNames.Version] == BsonNull.Value ||
                    doc[PropertyNames.Version][major] < toVersion.Major || 
                    (doc[PropertyNames.Version][major] == toVersion.Major && doc[PropertyNames.Version][minor] < toVersion.Minor) ||
                    (doc[PropertyNames.Version][major] == toVersion.Major && doc[PropertyNames.Version][minor] == toVersion.Minor && doc[PropertyNames.Version][revision] < toVersion.Revision),
                options: new FindOptions<BsonDocument>
                {
                    BatchSize = batchSize,
                },
                cancellationToken: cancellationToken);
    }
}