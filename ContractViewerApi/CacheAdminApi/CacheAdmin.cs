using Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

public static class CacheAdmin
{
    public static void MapCacheEndpoints(this WebApplication app)
    {
        app.MapGet("cache", GetCache).WithOpenApi().WithName(nameof(GetCache));
        app.MapPost("cache", AddCacheItem).WithOpenApi().WithName(nameof(AddCacheItem));
        app.MapDelete("cache/{key}", DeleteCacheItem).WithOpenApi().WithName(nameof(DeleteCacheItem));
        app.MapDelete("cache/clear", ClearCache).WithOpenApi().WithName(nameof(ClearCache));
    }

    private static async Task<IResult> GetCache(IConnectionMultiplexer redis, RedisTenantContext ctx)
    {
        var server = redis.GetServer(redis.GetEndPoints().First());
        var allEntries = new Dictionary<string, string?>();
        foreach (var dbIndex in Enumerable.Range(0, server.DatabaseCount))
        {
            var db = redis.GetDatabase(dbIndex);

            await foreach (var key in server.KeysAsync(database: db.Database, pattern: ctx.AnyKeyPattern))
            {
                var value = await db.StringGetAsync(key);
                allEntries[key!] = value.HasValue ? value.ToString() : null;
            }
        }

        return Results.Ok(allEntries);
    }

    private static async Task<IResult> DeleteCacheItem(string key, IConnectionMultiplexer connection)
    {
        key = Uri.UnescapeDataString(key);
        if (string.IsNullOrWhiteSpace(key))
        {
            return TypedResults.BadRequest("Key must be provided.");
        }

        var db = connection.GetDatabase();
        var deleted = await db.KeyDeleteAsync(key);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    private static async Task<IResult> AddCacheItem([FromBody] AddCacheRequest req, IConnectionMultiplexer connection)
    {
        var key = Uri.UnescapeDataString(req.Key);
        if (string.IsNullOrWhiteSpace(key))
        {
            return TypedResults.BadRequest("Key must be provided.");
        }
        var db = connection.GetDatabase();
        try
        {
            await db.StringSetAsync(key, req.Value);
        }
        catch (RedisServerException e) when (e.Message.Contains("NOPERM"))
        {
            return TypedResults.Unauthorized();
        }
        return Results.NoContent();
    }

    private static async Task<NoContent> ClearCache(IConnectionMultiplexer redis, RedisTenantContext ctx)
    {
        await redis.ClearCacheAsync(ctx);
        return TypedResults.NoContent();
    }
}