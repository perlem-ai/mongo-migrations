using Hdp.Infrastructure.Mongo.Migrations.Registries;
using Hdp.Versioning;

namespace Hdp.Infrastructure.Mongo.Migrations.Services;

public class DocumentCurrentVersionProvider<TDocument>
    where TDocument : IVersioned
{
    private readonly DocumentMigrationSequence<TDocument> _documentMigrationSequence;

    public DocumentCurrentVersionProvider(DocumentMigrationSequence<TDocument> documentMigrationSequence)
    {
        _documentMigrationSequence = documentMigrationSequence;
    }

    public DataVersion? Get() 
        => _documentMigrationSequence.LastOrDefault()?.GetVersion();
}