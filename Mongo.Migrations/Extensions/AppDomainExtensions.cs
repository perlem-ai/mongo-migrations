namespace Hdp.Infrastructure.Mongo.Migrations.Extensions;

public static class AppDomainExtensions
{
    public static IReadOnlyCollection<Type> GetInterfaceImplementations<TInterfaceType>(this AppDomain appDomain)
        => (from assembly in appDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where typeof(TInterfaceType).IsAssignableFrom(type) && !type.IsAbstract
                select type)
            .Distinct()
            .ToArray();
}