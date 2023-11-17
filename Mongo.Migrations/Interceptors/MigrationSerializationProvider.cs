using Hdp.Infrastructure.Mongo.Migrations.Registries;
using Hdp.Infrastructure.Mongo.Migrations.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;

namespace Hdp.Infrastructure.Mongo.Migrations.Interceptors;

public class MigrationSerializationProvider : IMigrationSerializationProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MongoMigrationOptions _migrationOptions;

    public MigrationSerializationProvider(IServiceProvider serviceProvider, IOptions<MongoMigrationOptions> migrationOptions)
    {
        _serviceProvider = serviceProvider;
        _migrationOptions = migrationOptions.Value;
    }
    
    public IBsonSerializer? GetSerializer(Type documentType)
    {
        if (!DocumentMigrationsRegistry.HasMigrations(documentType))
            return null;

        var interceptorType = typeof(DefaultMigrationSerializationInterceptor<>);
        
        return ResolveSerializer(_serviceProvider, interceptorType, documentType) as IBsonSerializer;
        
        static object? ResolveSerializer(IServiceProvider serviceProvider, Type interceptorType, Type documentType) 
            => serviceProvider.GetService(interceptorType.MakeGenericType(documentType));
    }
}