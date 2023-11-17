using Hdp.Infrastructure.Mongo.Migrations.Services;
using Hdp.Versioning;
using MongoDB.Driver;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrations.Collection;

public abstract class UpdateDefinitionCollectionMigration<TDocument> : IBatchCollectionMigration<TDocument>
    where TDocument : IVersioned
{
    private readonly DocumentCurrentVersionProvider<TDocument> _versionProvider;
    private readonly CollectionLocator<TDocument> _collectionLocator;

    public UpdateDefinitionCollectionMigration(DocumentCurrentVersionProvider<TDocument> versionProvider,
        CollectionLocator<TDocument> collectionLocator)
    {
        _versionProvider = versionProvider;
        _collectionLocator = collectionLocator;
    }

    public async Task Up(CancellationToken cancellationToken)
    {
        var currentVersion = _versionProvider.Get();
        if (currentVersion is null)
            return;

        await _collectionLocator.Get()
            .UpdateManyAsync(doc => doc.Version == VersionFilter,
                UpDefinition.Set(doc => doc.Version, currentVersion), cancellationToken: cancellationToken);
    }

    public Task Down(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected abstract UpdateDefinition<TDocument> UpDefinition { get; }
    
    protected abstract UpdateDefinition<TDocument> DownDefinition { get; }
    
    public abstract DataVersion Version { get; }
    
    public abstract DataVersion VersionFilter { get; }
}