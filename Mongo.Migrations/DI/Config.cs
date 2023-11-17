using Hdp.Infrastructure.Mongo.Migrations.Background;
using Hdp.Infrastructure.Mongo.Migrations.Contracts.Attributes;
using Hdp.Infrastructure.Mongo.Migrations.Contracts.Interfaces;
using Hdp.Infrastructure.Mongo.Migrations.Interceptors;
using Hdp.Infrastructure.Mongo.Migrations.Migrations.Collection;
using Hdp.Infrastructure.Mongo.Migrations.Migrations.Database;
using Hdp.Infrastructure.Mongo.Migrations.Migrations.Document;
using Hdp.Infrastructure.Mongo.Migrations.Migrators;
using Hdp.Infrastructure.Mongo.Migrations.Registries;
using Hdp.Infrastructure.Mongo.Migrations.Services;
using Hdp.Infrastructure.Mongo.Migrations.Settings;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Hdp.Infrastructure.Mongo.Migrations.DI;

public static class Config
{
    public static IServiceCollection AddMongoRuntimeMigrations(this IServiceCollection services)
    {
        return services.RegisterRuntimeMigrations();
    }

    public static IServiceCollection AddMongoCollectionMigrations<TCollectionNameResolver, TDocumentSettingsProvider>(this IServiceCollection services)
        where TCollectionNameResolver : class, ICollectionNameResolver 
        where TDocumentSettingsProvider : class, IDocumentSettingsProvider
    {
        return services.RegisterCollectionMigrations<TCollectionNameResolver, TDocumentSettingsProvider>();
    }

    public static IServiceCollection AddMongoBackgroundJobMigrations<TCollectionNameResolver, TDocumentSettingsProvider>(this IServiceCollection services, 
        Action<BackgroundMigrationsSettings> backgroundMigrationConfigureAction)
        where TCollectionNameResolver : class, ICollectionNameResolver 
        where TDocumentSettingsProvider : class, IDocumentSettingsProvider
    {
        services.RegisterRuntimeMigrations();
        services.RegisterCollectionMigrations<TCollectionNameResolver, TDocumentSettingsProvider>();
        services.RegisterDatabaseMigrations();
        services.Configure(backgroundMigrationConfigureAction);

        services.AddHostedService<BackgroundMigrationService>();

        return services;
    }

    private static IServiceCollection RegisterRuntimeMigrations(this IServiceCollection services)
    {
        services.AddTransient(typeof(DocumentCurrentVersionProvider<>));
        services.AddTransient(typeof(SingleDocumentMigrator<>));
        services.AddTransient(typeof(DocumentMigrationSequence<>));
            
        services.AddTransient(typeof(DefaultMigrationSerializationInterceptor<>));
        
        ValidateMigrations(DocumentMigrationsRegistry.AllMigrations);
        foreach (var documentType in DocumentMigrationsRegistry.DocumentsWithMigrations)
        {
            foreach (var migrationType in DocumentMigrationsRegistry.GetMigrations(documentType))
            {
                services.AddTransient(typeof(IDocumentMigration<>).MakeGenericType(documentType),
                    migrationType);
            }
        }
        services.AddTransient<IMigrationSerializationProvider, MigrationSerializationProvider>();
        // Registering serialization provider before application starts to be sure that all hosted services resolved correctly.
        var serializationProvider = services.BuildServiceProvider().GetRequiredService<IMigrationSerializationProvider>();
        BsonSerializer.RegisterSerializationProvider(serializationProvider);
        
        return services;
    }

    private static IServiceCollection RegisterCollectionMigrations<TCollectionNameResolver, TDocumentSettingsProvider>(this IServiceCollection services)
        where TCollectionNameResolver : class, ICollectionNameResolver
        where TDocumentSettingsProvider : class, IDocumentSettingsProvider
    {
        ValidateMigrations(BatchCollectionMigrationsRegistry.AllMigrations);
        foreach (var documentType in DocumentMigrationsRegistry.DocumentsWithMigrations)
        {
            foreach (var migrationType in BatchCollectionMigrationsRegistry.GetMigrations(documentType))
            {
                services.AddTransient(typeof(IBatchCollectionMigration<>).MakeGenericType(documentType),
                    migrationType);
            }
        }
        
        services.AddSingleton(typeof(GranularCollectionMigrator<>));
        services.AddSingleton(typeof(BatchCollectionMigrationSequence<>));
        services.AddSingleton(typeof(BatchCollectionMigrator<>));

        services.AddSingleton<MigrationRunner>();
        services.AddSingleton<ICollectionNameResolver, TCollectionNameResolver>();
        services.AddSingleton<IDocumentSettingsProvider, TDocumentSettingsProvider>();
        services.AddSingleton(typeof(CollectionLocator<>));
        
        return services;
    }

    private static IServiceCollection RegisterDatabaseMigrations(this IServiceCollection services)
    {
        ValidateMigrations(DatabaseMigrationsRegistry.Migrations);
        foreach (var databaseMigration in DatabaseMigrationsRegistry.Migrations)
        {
            services.AddTransient(typeof(IDatabaseMigration), databaseMigration);
        }

        services.AddSingleton<DatabaseMigrationsSequence>();
        services.AddSingleton<DatabaseMigrator>();
        services.AddSingleton<DatabaseMigrationHistoryService>();

        return services;
    }


    private static void ValidateMigrations(IReadOnlyCollection<Type> migrations)
    {
        var expiredMigrations = from migration in migrations
            let attributes = migration.GetCustomAttributes(false)
            let transientAttribute = attributes.OfType<TransientMigrationAttribute>().SingleOrDefault()
            where transientAttribute is not null
            where transientAttribute.ExpirationDate < DateOnly.FromDateTime(DateTime.UtcNow)
            select migration.Name;
        
        var expiredMigrationNames = string.Join(", ", expiredMigrations);
        if (!string.IsNullOrWhiteSpace(expiredMigrationNames))
        {
            throw new Exception($"Migration(s) [{expiredMigrationNames}] are expired. " +
                                $"Remove them or extend their expiration date in {nameof(TransientMigrationAttribute)}");
        }
    }
}