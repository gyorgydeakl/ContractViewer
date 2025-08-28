using System.Text.Json;
using StackExchange.Redis;

namespace ContractViewerApi;

public static class RedisExtensions
{
    /// <summary>
    /// Attempts to retrieve a cached value from Redis and deserialize it into <typeparamref name="T"/>.
    /// If the key is not found in the cache, the provided factory function is executed to produce the value,
    /// which is then serialized and stored in Redis before being returned.
    /// </summary>
    /// <typeparam name="T">The reference type to deserialize the cached value into.</typeparam>
    /// <param name="db">The Redis database instance.</param>
    /// <param name="key">The cache key to look up.</param>
    /// <param name="factory">An async factory function used to generate the value if the cache does not contain it.</param>
    /// <returns>
    /// The cached or newly generated instance of <typeparamref name="T"/>.
    /// </returns>
    public static async Task<T> TryGetDeserialized<T>(this IDatabase db, string key, Func<Task<T>> factory)
        where T : class
    {
        var cacheResult = await db.StringGetAsync(key);
        if (cacheResult.HasValue)
        {
            return JsonSerializer.Deserialize<T>(cacheResult!)!;
        }

        var result = await factory();
        await db.StringSetAsync(key, JsonSerializer.Serialize(result));
        return result;
    }
    public static async Task ClearCacheAsync(this IConnectionMultiplexer redis)
    {
        const int dbNumber = 0;
        foreach (var endpoint in redis.GetEndPoints())
        {
            await redis.GetServer(endpoint).FlushDatabaseAsync(dbNumber);
        }
    }
}