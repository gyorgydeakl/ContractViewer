 using System.Security.Claims;
 using Common;
 using ContractViewerApi;
 using Microsoft.AspNetCore.DataProtection;
 using Microsoft.AspNetCore.Identity;
 using Microsoft.OpenApi.Models;
 using Scalar.AspNetCore;
 using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Components ??= new OpenApiComponents();
        doc.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "JWT in the form: Bearer {token}"
        };

        doc.SecurityRequirements ??= [];
        doc.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/shared-keys"))
    .SetApplicationName("ContractViewerAuth");
builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultChallengeScheme = IdentityConstants.BearerScheme;
        opt.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
    })
    .AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisCache");
    options.InstanceName = "contractViewerTenant:";
});
builder.Services.AddOutputCache();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("RedisCache")!;
    return ConnectionMultiplexer.Connect(connectionString);
});
builder.Services.AddSingleton<RedisTenantContext>(sp =>
{
    var username = sp.GetRequiredService<IConnectionMultiplexer>()
        .GetDatabase()
        .Execute("ACL", "WHOAMI")
        .ToString()
        .Trim();
    return new RedisTenantContext(username, $"{username}:");
});
builder.Services.AddHttpClientForApi(Apis.ContractDetails);
builder.Services.AddHttpClientForApi(Apis.ContractList);
builder.Services.AddHttpClientForApi(Apis.User);
builder.Services.AddHttpClientForApi(Apis.Poa);

builder.Services.AddCors(options => 
    options.AddPolicy("AllowAngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapAppEndpoints();
app.Run();