using Hdp.Infrastructure.Mongo.Migrations.Extensions;
using Hdp.Infrastructure.Mongo.Migrations.Migrations.Document;

namespace Hdp.Infrastructure.Mongo.Migrations.Registries;

public static class DocumentMigrationsRegistry
{
    private static readonly HashSet<Type> _documentsWithMigration;

    private static readonly Dictionary<Type, List<Type>> _documentMigrations = new ();

    public static IReadOnlyCollection<Type> DocumentsWithMigrations
        => _documentsWithMigration;

    public static IReadOnlyCollection<Type> AllMigrations { get; }

    static DocumentMigrationsRegistry()
    {
        var migrationTypes = AppDomain.CurrentDomain.GetInterfaceImplementations<IDocumentMigration>();

        foreach (var migrationType in migrationTypes)
        {
            var documentType = migrationType.BaseType?.GenericTypeArguments[0] ?? throw new NullReferenceException();
            if (_documentMigrations.ContainsKey(documentType))
            {
                _documentMigrations[documentType].Add(migrationType);
            }
            else
            {
                _documentMigrations[documentType] = new List<Type> { migrationType };
            }
        }

        _documentsWithMigration = _documentMigrations
            .Select(d => d.Key)
            .ToHashSet();

        AllMigrations = migrationTypes;
    }

    public static bool HasMigrations(Type documentType) 
        => _documentMigrations.ContainsKey(documentType);

    public static IReadOnlyCollection<Type> GetMigrations(Type documentType)
        => _documentMigrations.ContainsKey(documentType)
            ? _documentMigrations[documentType]
            : new List<Type>();
}