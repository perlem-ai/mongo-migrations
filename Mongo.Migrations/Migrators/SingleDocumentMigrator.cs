using Hdp.Infrastructure.Mongo.Migrations.Constants;
using Hdp.Infrastructure.Mongo.Migrations.Registries;
using Hdp.Versioning;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrators;

internal class SingleDocumentMigrator<TDocument>
where TDocument : IVersioned
{
    private readonly DocumentMigrationSequence<TDocument> _documentMigrations;

    public SingleDocumentMigrator(DocumentMigrationSequence<TDocument> documentMigrations)
    {
        _documentMigrations = documentMigrations;
    }

    public BsonDocument Migrate(BsonDocument document)
    {
        document.TryGetValue(PropertyNames.Version, out var versionValue);

        var documentVersion = versionValue is null or BsonNull
            ? DataVersion.Empty()
            : BsonSerializer.Deserialize<DataVersion>(versionValue.AsBsonDocument);
        
        var migrationsToApply = _documentMigrations
            .Where(m => m.GetVersion() > documentVersion);

        foreach (var migration in migrationsToApply)
        {
            document = migration.Up(document);
            document.Set(PropertyNames.Version, migration.GetVersion().ToBsonDocument());
        }

        return document;
    }
}