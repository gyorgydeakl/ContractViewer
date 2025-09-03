using System.Security.Claims;
using Common;
using ContractDetailsApi;
using ContractListApi;
using PowerOfAttorneyApi;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using UserApi;

namespace ContractViewerApi;

public static class ContractViewer
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // cache
        app.MapGet("cache", GetCache).WithOpenApi().WithName(nameof(GetCache));
        app.MapPost("cache", AddCacheItem).WithOpenApi().WithName(nameof(AddCacheItem));
        app.MapDelete("cache/{key}", DeleteCacheItem).WithOpenApi().WithName(nameof(DeleteCacheItem));
        app.MapDelete("cache/clear", ClearCache).WithOpenApi().WithName(nameof(ClearCache));

        // user
        app.MapPost("login", Login).WithOpenApi().WithName(nameof(Login));
        app.MapPost("users", GetUsers).WithOpenApi().WithName(nameof(GetUsers));
        
        // contracts
        app.MapPost("contracts/generate", GenerateContracts)
            .RequireAuthorization()
            .WithOpenApi()
            .WithName(nameof(GenerateContracts));
        app.MapGet("contracts", GetContracts)
            .RequireAuthorization()
            .WithOpenApi()
            .WithName(nameof(GetContracts));
        app.MapGet("contract/{contractId}", GetContract)
            .RequireAuthorization()
            .WithOpenApi()
            .CacheOutput(policy => policy
                .AddPolicy<AllowAuthPerUserPolicy>()
                .Expire(TimeSpan.FromHours(1))
                .SetVaryByRouteValue("contractId")
                .VaryByValue(ctx => new KeyValuePair<string, string>(
                    "email",
                    ctx.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty)))
            .WithName(nameof(GetContract));
        
        // poas
        app.MapPost("poas/generate", GeneratePoas).WithOpenApi().WithName(nameof(GeneratePoas));
        app.MapGet("poasGrantedByMe", GetPoasGrantedByMe).WithOpenApi().WithName(nameof(GetPoasGrantedByMe));
        app.MapGet("poasGrantedForMe", GetPoasGrantedForMe).WithOpenApi().WithName(nameof(GetPoasGrantedForMe));
    }

    private static Task<User[]> GetUsers(IHttpClientFactory clientFactory)
    {
        return clientFactory.GetClient(Apis.User).GetAsync<User[]>("users");
    }

    private static async Task<IResult> GetCache(IConnectionMultiplexer redis, RedisTenantContext ctx)
    {
        var server = redis.GetServer(redis.GetEndPoints().First());
        var db = redis.GetDatabase();

        var allEntries = new Dictionary<string, string?>();
        await foreach (var key in server.KeysAsync(database: db.Database, pattern: ctx.AnyKeyPattern))
        {
            var value = await db.StringGetAsync(key);
            allEntries[key!] = value.HasValue ? value.ToString() : null;
        }

        return Results.Ok(allEntries);
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

    private static async Task<NoContent> ClearCache(IConnectionMultiplexer redis, RedisTenantContext ctx)
    {
        await redis.ClearCacheAsync(ctx);
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

    private static async Task<ContractSummary[]> GenerateContracts(
        HttpContext httpCtx, 
        [FromBody] GenerateContractRequest req,
        IHttpClientFactory clientFactory,
        IConnectionMultiplexer redis,
        RedisTenantContext tenantCtx)
    {
        var myEmail = httpCtx.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        var myUserId = await redis.GetDatabase().TryGetDeserialized(
            tenantCtx.CreateKey($"{myEmail}/userId"), 
            () => clientFactory.GetClient(Apis.User, httpCtx).GetAsync<string>("userId"));
        var contracts = await clientFactory
            .GetClient(Apis.ContractList)
            .PostAsync<GenerateContractRequest, ContractSummary[]>($"user/{myUserId}/contracts/random", req);
        await redis.ClearCacheAsync(tenantCtx);
        return contracts;
    }

    private static async Task<Ok<ContractSummary[]>> GetContracts(
        HttpContext httpCtx,
        IHttpClientFactory clientFactory,
        IConnectionMultiplexer redis,
        RedisTenantContext tenantCtx)
    {
        var myUserId = await httpCtx.GetUserId(clientFactory, redis, tenantCtx);
        var contracts = await redis.GetDatabase().TryGetDeserialized(
            tenantCtx.CreateKey($"user/{myUserId}/contracts"), 
            () => clientFactory.GetClient(Apis.ContractList).GetAsync<ContractSummary[]>($"user/{myUserId}/contracts"));
        return TypedResults.Ok(contracts);
    }

    private static Task<ContractDetails> GetContract(string contractId, IHttpClientFactory clientFactory)
    {
        return clientFactory.GetClient(Apis.ContractDetails).GetAsync<ContractDetails>($"contract/{contractId}");
    }

    private static async Task<PowerOfAttorneyDto[]> GeneratePoas(
        [FromBody] GeneratePoaRequest req,
        IHttpClientFactory clientFactory, 
        IConnectionMultiplexer redis,
        RedisTenantContext ctx)
    {
        var poas = await clientFactory
            .GetClient(Apis.Poa)
            .PostAsync<GeneratePoaRequest, PowerOfAttorneyDto[]>($"poas/generate", req);
        await redis.ClearCacheAsync(ctx);
        return poas;
    }

    private static async Task<PowerOfAttorneyDto[]> GetPoasGrantedByMe(
        HttpContext httpCtx, 
        IHttpClientFactory clientFactory,
        IConnectionMultiplexer redis,
        RedisTenantContext tenantCtx)
    {
        var userId = await httpCtx.GetUserId(clientFactory, redis, tenantCtx);
        return await redis.GetDatabase().TryGetDeserialized(
            tenantCtx.CreateKey($"user/{userId}/poasGrantedByMe"),
            () => clientFactory.GetClient(Apis.Poa).GetAsync<PowerOfAttorneyDto[]>($"poas?grantorId={userId}"));
    }
    
    
    private static async Task<PowerOfAttorneyDto[]> GetPoasGrantedForMe(
        HttpContext httpCtx, 
        IHttpClientFactory clientFactory,
        IConnectionMultiplexer redis,
        RedisTenantContext tenantCtx)
    {
        var userId = await httpCtx.GetUserId(clientFactory, redis, tenantCtx);
        return await redis.GetDatabase().TryGetDeserialized(
            tenantCtx.CreateKey($"user/{userId}/poasGratedForMe"),
            () => clientFactory.GetClient(Apis.Poa).GetAsync<PowerOfAttorneyDto[]>($"poas?delegateId={userId}"));
    }

    private static Task<string> GetUserId(this HttpContext httpCtx,
        IHttpClientFactory clientFactory,
        IConnectionMultiplexer redis,
        RedisTenantContext tenantCtx)
    {
        var myEmail = httpCtx.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        return redis.GetDatabase().TryGetDeserialized(
            tenantCtx.CreateKey($"{myEmail}/userId"), 
            () => clientFactory.GetClient(Apis.User, httpCtx).GetAsync<string>("userId"));
    }
}