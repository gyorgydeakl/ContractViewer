using System.Text.Json;
using StackExchange.Redis;

namespace ContractViewerApi;

public static class RedisExtensions
{
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
    
    public static async Task<T?> TryGetDeserialized<T>(this IDatabase db, string key)
        where T : class
    {
        var cacheResult = await db.StringGetAsync(key);
        return cacheResult.HasValue ? JsonSerializer.Deserialize<T>(cacheResult!) : null;
    }
}