using Hdp.Versioning;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrations.Document;

public abstract class DocumentBsonMigration<TDocument> : DocumentMigrationBase<TDocument>
    where TDocument : IVersioned
{
}
