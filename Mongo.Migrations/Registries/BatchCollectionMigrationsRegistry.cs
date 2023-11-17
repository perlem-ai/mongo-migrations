using Hdp.Infrastructure.Mongo.Migrations.Extensions;
using Hdp.Infrastructure.Mongo.Migrations.Migrations.Collection;

namespace Hdp.Infrastructure.Mongo.Migrations.Registries;

internal static class BatchCollectionMigrationsRegistry
{
    private static readonly Dictionary<Type, List<Type>> _documentMigrations = new ();
    private static readonly HashSet<Type> _documentsWithMigration;

    static BatchCollectionMigrationsRegistry()
    {
        var migrations = AppDomain.CurrentDomain.GetInterfaceImplementations<IBatchCollectionMigration>(); 

        foreach (var migrationType in migrations)
        {
            var documentType = migrationType.BaseType?.GenericTypeArguments[0] ?? throw new NullReferenceException();
            if (_documentMigrations.TryGetValue(documentType, out var migration))
            {
                migration.Add(migrationType);
            }
            else
            {
                _documentMigrations[documentType] = new List<Type> { migrationType };
            }
        }
        
        _documentsWithMigration = _documentMigrations
            .Select(d => d.Key)
            .ToHashSet();

        AllMigrations = migrations;
    }

    public static IReadOnlyCollection<Type> DocumentsWithMigrations
        => _documentsWithMigration;

    public static IEnumerable<Type> GetMigrations(Type documentType)
        => _documentMigrations.ContainsKey(documentType)
            ? _documentMigrations[documentType]
            : new List<Type>();
    
    public static IReadOnlyCollection<Type> AllMigrations { get; }
}