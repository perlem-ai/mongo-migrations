namespace Hdp.Infrastructure.Mongo.Migrations.Registries;

public static class DistinctTypeCollectionExtensions
{
    public static IEnumerable<T> DistinctByType<T>(this IEnumerable<T> initial)
    {
        return initial
            .GroupBy(i => i?.GetType())
            .Select(i => i.First());
    }
}