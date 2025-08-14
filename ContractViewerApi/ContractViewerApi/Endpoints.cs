using System.Text.Json;
using StackExchange.Redis;

namespace ContractViewerApi;

public static class Endpoints
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapCacheEndpoints();
        app.MapContractEndpoints();
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

    private static void MapContractEndpoints(this WebApplication app)
    {
        var contractDetails = new Dictionary<string, ContractDetails>()
        {
            { "123456789", new() { ContractId = "123456789", Role = "Contractor", RegistrationNumber = "123456789" } },
            { "987654321", new() { ContractId = "987654321", Role = "Contractor", RegistrationNumber = "987654321" } },
        };
        var contracts = new List<ContractSummary>()
        {
            new() { ContractId = "123456789", Role = "Contractor", RegistrationNumber = "123456789" },
            new() { ContractId = "987654321", Role = "Contractor", RegistrationNumber = "987654321" },
        };
        app.MapGet("user/{userId}/contracts", async (string userId, IConnectionMultiplexer redis) =>
            {
                var key = $"user/{userId}/contracts";
                var db = redis.GetDatabase();
                var contractCache = await db.StringGetAsync(key);
                if (contractCache.HasValue)
                {
                    var result = JsonSerializer.Deserialize<List<ContractSummary>>(contractCache.ToString());
                    return TypedResults.Ok(result);
                }

                await db.StringSetAsync(key, JsonSerializer.Serialize(contracts));
                return TypedResults.Ok(contracts);
            })
            .WithOpenApi()
            .WithName("GetContracts");

        app.MapGet("contract/{contractId}", async (string contractId, IConnectionMultiplexer redis) =>
        {
            var key = $"contract/{contractId}";
            var db = redis.GetDatabase();
            var contractCache = db.StringGet(key);
            if (contractCache.HasValue)
            {
                var result = JsonSerializer.Deserialize<ContractDetails>(contractCache.ToString());
                return TypedResults.Ok(result);
            }

            await db.StringSetAsync(key, JsonSerializer.Serialize(contractDetails[contractId]));
            return TypedResults.Ok(contractDetails[contractId]);
        })
        .WithOpenApi()
        .WithName("GetContract");
    }
}

public record CacheItem(string Key, string Value);