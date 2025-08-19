using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using ContractListApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
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
                key = Uri.UnescapeDataString(key);
                if (string.IsNullOrWhiteSpace(key))
                {
                    return TypedResults.BadRequest("Key must be provided.");
                }

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
        app.MapPost("contracts/random", async (HttpContext ctx, [FromBody] GenerateContractRequest req, IHttpClientFactory clientFactory, IConnectionMultiplexer redis) =>
        {
            var myEmail = ctx.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var myUserId = await redis.GetDatabase().TryGetDeserialized(
                $"{myEmail}/userId",
                () => clientFactory.GetClient(Apis.User, ctx).GetAsync<string>("userId"));
            return await clientFactory
                .GetClient(Apis.ContractList)
                .PostAsync<GenerateContractRequest, ContractSummary[]>($"user/{myUserId}/contracts/random", req);
        })
        .RequireAuthorization()
        .WithOpenApi()
        .WithName("GenerateRandomContracts");
        
        app.MapGet("contracts", async (HttpContext ctx, IHttpClientFactory clientFactory, IConnectionMultiplexer redis) =>
        {
            var myEmail = ctx.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var myUserId = await redis.GetDatabase().TryGetDeserialized(
                $"{myEmail}/userId",
                () => clientFactory.GetClient(Apis.User, ctx).GetAsync<string>("userId"));
            var contracts = await redis.GetDatabase().TryGetDeserialized(
                $"user/{myUserId}/contracts", 
                () => clientFactory.GetClient(Apis.ContractList).GetAsync<ContractSummary[]>($"user/{myUserId}/contracts"));
            return TypedResults.Ok(contracts);
        })
        .RequireAuthorization()
        .WithOpenApi()
        .WithName("GetContracts");


        app.MapGet("contract/{contractId}", async (string contractId, ContractDetailsService contractDetailsService) =>
        {
            var contractDetails = await contractDetailsService.GetContractDetailsAsync(contractId);
            return TypedResults.Ok(contractDetails);
        })
        .WithOpenApi()
        .WithName("GetContract");
    }

    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("login", async ([FromBody] LoginRequest req, IHttpClientFactory clientFactory) => await clientFactory
            .GetClient(Apis.User)
            .PostAsync<LoginRequest, AccessTokenResponse>("login", new LoginRequest
            {
                Email = req.Email,
                Password = req.Password
            }))
        .WithOpenApi()
        .WithName("Login");
    }

    public static async Task<T> GetAsync<T>(this HttpClient client, string url) where T : class
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        if (jsonResponse is T tResponse)
        {
            return tResponse;
        }
        return JsonSerializer.Deserialize<T>(jsonResponse, JsonSerializerOptions.Web) ?? throw new Exception("Could not deserialize response.");
    }
    
    public static async Task<TRes> PostAsync<TReq, TRes>(this HttpClient client, string url, TReq body)
    {
        using var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        var str = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TRes>(str, JsonSerializerOptions.Web);
        return result ?? throw new InvalidOperationException("Could not deserialize response.");
    }
}

public record CacheItem(string Key, string Value);