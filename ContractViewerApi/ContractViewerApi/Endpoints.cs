using System.Text.Json;
using Common;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace ContractViewerApi;

public static class Endpoints
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapCacheEndpoints();
        app.MapUserEndpoints();
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
        var contractListApiAddress = new Uri("http://localhost:" + ContractListApi.Connection.Port);
        var contractDetailsApiAddress = new Uri("http://localhost:" + ContractDetailsApi.Connection.Port);

        app.MapGet("user/{userId}/contracts", async (string userId, HttpClient client, IConnectionMultiplexer redis) =>
            {
                var key = $"user/{userId}/contracts";
                var db = redis.GetDatabase();
                var contractCache = await db.StringGetAsync(key);
                if (contractCache.HasValue)
                {
                    var result = JsonSerializer.Deserialize<List<ContractSummary>>(contractCache.ToString());
                    return TypedResults.Ok(result);
                }
                client.BaseAddress = contractListApiAddress;

                var contracts = User.GetByUserId(userId)?
                    .Contracts
                    .Select(c => c.ToSummary())
                    .ToList() ?? [];
                await db.StringSetAsync(key, JsonSerializer.Serialize(contracts));
                return TypedResults.Ok(contracts);
            })
            .WithOpenApi()
            .WithName("GetContracts");

        app.MapGet("contract/{contractId}", async (string contractId, HttpClient client, IConnectionMultiplexer redis) =>
            {
                var key = $"contract/{contractId}";
                var db = redis.GetDatabase();
                var contractCache = db.StringGet(key);
                if (contractCache.HasValue)
                {
                    var result = JsonSerializer.Deserialize<ContractDetails>(contractCache.ToString());
                    return TypedResults.Ok(result);
                }
                
                client.BaseAddress = contractDetailsApiAddress;
                var contractDetails = await client.HttpGetAsync<ContractDetails>("contract/" + contractId);
                    
                await db.StringSetAsync(key, JsonSerializer.Serialize(contractDetails));
                return TypedResults.Ok(contractDetails);
            })
            .WithOpenApi()
            .WithName("GetContract");
    }

    public static void MapUserEndpoints(this WebApplication app)
    {
        var userApiAddress = new Uri("http://localhost:" + UserApi.Connection.Port);
        app.MapGet("login", async ([FromQuery] string username, [FromQuery] string password, HttpClient client, IConnectionMultiplexer redis) =>
        {
            client.BaseAddress = userApiAddress;
            
            var key = "username/" + username;
            var db = redis.GetDatabase();
            
            var userIdCache = await db.StringGetAsync(key);
            if (userIdCache.HasValue)
            {
                return TypedResults.Ok(userIdCache.ToString());
            }

            var userId = await client.HttpGetAsync<string>("login?username=" + username + "&password=" + password);
            await db.StringSetAsync(key, userId);
            
            return TypedResults.Ok(userId);
        })
        .WithOpenApi()
        .WithName("Login");

        app.MapGet("users", async (HttpClient client) =>
        {
            client.BaseAddress = userApiAddress;
            return await client.HttpGetAsync<Dictionary<string, string>>("users");
        })
        .WithOpenApi()
        .WithName("GetUsers");
    }

    public static async Task<T> HttpGetAsync<T>(this HttpClient client, string url) where T : class
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonResponse) ?? throw new Exception("Could not deserialize response.");
    }
}

public record CacheItem(string Key, string Value);
