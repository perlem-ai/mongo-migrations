using Hdp.Versioning;
using MongoDB.Bson;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrations.Document;

public interface IDocumentMigration<TDocument> : IDocumentMigration
    where TDocument: IVersioned
{
    DataVersion GetVersion();
    BsonDocument Up(BsonDocument document);
    BsonDocument Down(BsonDocument document);
}

public interface IDocumentMigration
{
}