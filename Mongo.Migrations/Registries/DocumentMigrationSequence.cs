using System.Collections;
using Hdp.Infrastructure.Mongo.Migrations.Migrations.Document;
using Hdp.Versioning;

namespace Hdp.Infrastructure.Mongo.Migrations.Registries;

public class DocumentMigrationSequence<TDocument> : IEnumerable<IDocumentMigration<TDocument>>
where TDocument : IVersioned
{
    private readonly IReadOnlyCollection<IDocumentMigration<TDocument>> _migrations;

    public DocumentMigrationSequence(IEnumerable<IDocumentMigration<TDocument>> migrations)
    {
        _migrations = migrations
            .OrderBy(m => m.GetVersion())
            .DistinctByType()
            .ToArray();
    }

    public IEnumerator<IDocumentMigration<TDocument>> GetEnumerator() 
        => _migrations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() 
        => GetEnumerator();
}