using Hdp.Infrastructure.Mongo.Migrations.Extensions;
using Hdp.Infrastructure.Mongo.Migrations.Migrations.Database;

namespace Hdp.Infrastructure.Mongo.Migrations.Registries;

internal static class DatabaseMigrationsRegistry
{
    public static IReadOnlyCollection<Type> Migrations 
        => AppDomain.CurrentDomain.GetInterfaceImplementations<IDatabaseMigration>();
}