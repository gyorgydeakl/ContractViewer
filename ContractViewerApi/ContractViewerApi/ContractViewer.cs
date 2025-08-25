using System.Security.Claims;
using System.Text.Json;
using Common;
using ContractDetailsApi;
using ContractListApi;
using DocumentApi;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace ContractViewerApi;

public static class ContractViewer
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // cache
        app.MapGet("cache", GetCache).WithOpenApi().WithName(nameof(GetCache));
        app.MapDelete("cache/{key}", DeleteCacheItem).WithOpenApi().WithName(nameof(DeleteCacheItem));
        app.MapDelete("cache/clear", ClearCache).WithOpenApi().WithName(nameof(ClearCache));

        // user
        app.MapPost("login", Login).WithOpenApi().WithName(nameof(Login));
        
        // contracts
        app.MapPost("contracts/generate", GenerateContracts)
            .RequireAuthorization()
            .WithOpenApi()
            .WithName(nameof(GenerateContracts));
        app.MapGet("contracts", GetContracts)
            .RequireAuthorization()
            .WithOpenApi()
            .WithName(nameof(GetContracts));
        app.MapGet("contract/{contractId}", GetContract).WithOpenApi().WithName(nameof(GetContract));
        
        // documents
        app.MapPost("documents/generate", GenerateDocuments).WithOpenApi().WithName(nameof(GenerateDocuments));
        app.MapPost("documents", GetDocuments).WithOpenApi().WithName(nameof(GetDocuments));
    }

    private static async Task<IResult> GetCache(IConnectionMultiplexer redis)
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

    private static async Task<NoContent> ClearCache(IConnectionMultiplexer redis)
    {
        await redis.ClearCacheAsync();
        return TypedResults.NoContent();
    }

    private static Task<AccessTokenResponse> Login([FromBody] LoginRequest req, IHttpClientFactory clientFactory)
    {
        return clientFactory.
            GetClient(Apis.User)
            .PostAsync<LoginRequest, AccessTokenResponse>("login", new LoginRequest
            {
                Email = req.Email,
                Password = req.Password
            });
    }

    private static async Task<ContractSummary[]> GenerateContracts(HttpContext ctx, [FromBody] GenerateContractRequest req, IHttpClientFactory clientFactory, IConnectionMultiplexer redis)
    {
        var myEmail = ctx.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        var myUserId = await redis.GetDatabase().TryGetDeserialized(
            $"{myEmail}/userId", 
            () => clientFactory.GetClient(Apis.User, ctx).GetAsync<string>("userId"));
        var contracts = await clientFactory
            .GetClient(Apis.ContractList)
            .PostAsync<GenerateContractRequest, ContractSummary[]>($"user/{myUserId}/contracts/random", req);
        await redis.ClearCacheAsync();
        return contracts;
    }

    private static async Task<Ok<ContractSummary[]>> GetContracts(HttpContext ctx, IHttpClientFactory clientFactory, IConnectionMultiplexer redis)
    {
        var myEmail = ctx.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        var myUserId = await redis.GetDatabase().TryGetDeserialized(
            $"{myEmail}/userId", 
            () => clientFactory.GetClient(Apis.User, ctx).GetAsync<string>("userId"));
        var contracts = await redis.GetDatabase().TryGetDeserialized(
            $"user/{myUserId}/contracts", 
            () => clientFactory.GetClient(Apis.ContractList).GetAsync<ContractSummary[]>($"user/{myUserId}/contracts"));
        return TypedResults.Ok(contracts);
    }

    private static Task<ContractDetails> GetContract(string contractId, IHttpClientFactory clientFactory, IConnectionMultiplexer redis)
    {
        return redis.GetDatabase()
            .TryGetDeserialized($"contract/{contractId}", async () => await clientFactory.GetClient(Apis.ContractDetails)
                .GetAsync<ContractDetails>($"contract/{contractId}"));
    }

    private static async Task<DocumentDto[]> GenerateDocuments([FromBody] GenerateDocumentRequest req, IHttpClientFactory clientFactory, IConnectionMultiplexer redis)
    {
        var docs = await clientFactory.GetClient(Apis.Document)
            .PostAsync<GenerateDocumentRequest, DocumentDto[]>("documents/generate", req);
        await redis.ClearCacheAsync();
        return docs;
    }

    private static Task<DocumentDto[]> GetDocuments([FromQuery] string contracts, IHttpClientFactory clientFactory, IConnectionMultiplexer redis)
    {
        if (string.IsNullOrWhiteSpace(contracts))
        {
            return Task.FromResult(Array.Empty<DocumentDto>());
        }
        return redis.GetDatabase().TryGetDeserialized(
            $"documents/{contracts}",
            () => clientFactory.GetClient(Apis.Document).GetAsync<DocumentDto[]>("documents?contracts=" + contracts));
    }

    private static async Task<T> GetAsync<T>(this HttpClient client, string url) where T : class
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        if (jsonResponse is T tResponse)
        {
            return tResponse;
        }
        return JsonSerializer.Deserialize<T>(jsonResponse, JsonSerializerOptions.Web) ?? 
               throw new Exception("Could not deserialize response.");
    }
    
    private static async Task<TRes> PostAsync<TReq, TRes>(this HttpClient client, string url, TReq body)
    {
        using var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        var str = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TRes>(str, JsonSerializerOptions.Web);
        return result ?? throw new InvalidOperationException("Could not deserialize response.");
    }

    private static async Task ClearCacheAsync(this IConnectionMultiplexer redis)
    {
        const int dbNumber = 0;
        foreach (var endpoint in redis.GetEndPoints())
        {
            await redis.GetServer(endpoint).FlushDatabaseAsync(dbNumber);
        }
    }
}