using MongoDB.Driver;

namespace Hdp.Infrastructure.Mongo.Migrations.Extensions;

internal static class AsyncCursorExtensions
{
    /// <summary>
    /// Allows to use IAsyncEnumerable with IAsyncCursor (duck typing)
    /// </summary>
    public static IAsyncCursor<T> GetAsyncEnumerator<T>(this IAsyncCursor<T> cursor) => cursor;
}