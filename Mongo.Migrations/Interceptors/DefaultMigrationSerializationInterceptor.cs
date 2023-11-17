using Hdp.Infrastructure.Mongo.Migrations.Migrators;
using Hdp.Infrastructure.Mongo.Migrations.Services;
using Hdp.Versioning;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Hdp.Infrastructure.Mongo.Migrations.Interceptors;

internal class DefaultMigrationSerializationInterceptor<TDocument> : BsonClassMapSerializer<TDocument> where TDocument : class, IVersioned
{
    private readonly DocumentCurrentVersionProvider<TDocument> _documentCurrentVersionProvider;
    private readonly SingleDocumentMigrator<TDocument> _migrator;

    public DefaultMigrationSerializationInterceptor(DocumentCurrentVersionProvider<TDocument> documentCurrentVersionProvider,
        SingleDocumentMigrator<TDocument> migrator)
        : base(BsonClassMap.LookupClassMap(typeof(TDocument)))
    {
        _documentCurrentVersionProvider = documentCurrentVersionProvider;
        _migrator = migrator;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TDocument value)
    {
        var currentVersion = _documentCurrentVersionProvider.Get();
        if (currentVersion is not null)
            value.Version = currentVersion;
        
        base.Serialize(context, args, value);
    }

    public override TDocument Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var currentVersion = _documentCurrentVersionProvider.Get();
        if (currentVersion is null)
            return base.Deserialize(context, args);
        
        var document = BsonDocumentSerializer.Instance.Deserialize(context, args);
        document = _migrator.Migrate(document);

        var migratedContext = BsonDeserializationContext.CreateRoot(new BsonDocumentReader(document));

        return base.Deserialize(migratedContext, args);
    }
}