using Hdp.Versioning;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrations.Document;

public abstract class DocumentTypedMigration<TDocument, TOldVersion, TNewVersion> : DocumentMigrationBase<TDocument>
    where TDocument : IVersioned
{
    public override BsonDocument Up(BsonDocument document)
    {
        var deserialized = BsonSerializer.Deserialize<TOldVersion>(document);
        var newVersion = Up(deserialized);

        return newVersion.ToBsonDocument();
    }

    public override BsonDocument Down(BsonDocument document)
    {
        var deserialized = BsonSerializer.Deserialize<TNewVersion>(document);
        var oldVersion = Down(deserialized);

        return oldVersion.ToBsonDocument();
    }

    public abstract TNewVersion Up(TOldVersion entity);

    public abstract TOldVersion Down(TNewVersion entity);
}