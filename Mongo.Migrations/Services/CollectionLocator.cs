using Hdp.Infrastructure.Mongo.Migrations.Contracts.Interfaces;
using Hdp.Versioning;
using MongoDB.Driver;

namespace Hdp.Infrastructure.Mongo.Migrations.Services;

public class CollectionLocator<TDocument>
    where TDocument : IVersioned
{
    private readonly IMongoDatabase _database;
    private readonly ICollectionNameResolver _collectionNameResolver;

    public CollectionLocator(IMongoDatabase database, ICollectionNameResolver collectionNameResolver)
    {
        _database = database;
        _collectionNameResolver = collectionNameResolver;
    }

    public IMongoCollection<TDocument> Get()
        => _database.GetCollection<TDocument>(_collectionNameResolver.GetName(typeof(TDocument)));
}