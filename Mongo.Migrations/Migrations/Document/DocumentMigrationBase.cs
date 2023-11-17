using Hdp.Versioning;
using MongoDB.Bson;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrations.Document;

public abstract class DocumentMigrationBase<TDocument> : IDocumentMigration<TDocument>
    where TDocument : IVersioned
{
    public abstract DataVersion GetVersion();

    public abstract BsonDocument Up(BsonDocument document);
    
    public abstract BsonDocument Down(BsonDocument document);
    
    public Type Type 
        => typeof (TDocument);
}