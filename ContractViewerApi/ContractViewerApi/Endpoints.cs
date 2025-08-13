using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace ContractViewerApi;

public static class Endpoints
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapCacheEndpoints();
    }

    private static void MapCacheEndpoints(this WebApplication app)
    {
        app.MapGet("cache", async (IConnectionMultiplexer redis) =>
        {

            var server = redis.GetServer(redis.GetEndPoints().First());
            var db = redis.GetDatabase();

            var allEntries = new Dictionary<string, string?>();
            await foreach (var key in server.KeysAsync(database: db.Database, pattern: "*"))
            {
                var value = await db.StringGetAsync(key);
                allEntries[key!] = value.HasValue ? value.ToString() : null;
            }

            return Results.Ok(allEntries);
        })
        .WithOpenApi()
        .WithName("GetCache");

        app.MapDelete("cache/{key}", async (string key, IConnectionMultiplexer connection) =>
        {
            if (string.IsNullOrWhiteSpace(key))
                return TypedResults.BadRequest("Key must be provided.");

            var db = connection.GetDatabase();
            var deleted = await db.KeyDeleteAsync(key);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithOpenApi()
        .WithName("DeleteCacheItem");

        app.MapDelete("cache/clear", async (IConnectionMultiplexer mux) =>
        {
            const int dbNumber = 0;

            foreach (var endpoint in mux.GetEndPoints())
            {
                await mux.GetServer(endpoint).FlushDatabaseAsync(dbNumber);
            }

            return TypedResults.NoContent();
        })
        .WithOpenApi()
        .WithName("ClearCache");
    }
}

public record CacheItem(string Key, string Value);