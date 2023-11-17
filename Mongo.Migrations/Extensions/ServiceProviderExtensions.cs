using Microsoft.Extensions.DependencyInjection;

namespace Hdp.Infrastructure.Mongo.Migrations.Extensions;

internal static class ServiceProviderExtensions
{
    public static TTargetType ResolveGeneric<TTargetType>(this IServiceProvider serviceProvider, Type genericType,
        Type typeArgument) where TTargetType : class
        => (TTargetType)serviceProvider.GetRequiredService(genericType.MakeGenericType(typeArgument));
}