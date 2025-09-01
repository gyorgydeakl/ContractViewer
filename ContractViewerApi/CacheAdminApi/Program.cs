using System.Net;
using CacheAdminApi;
using Common;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("RedisCache")!;
    return ConnectionMultiplexer.Connect(connectionString);
});
builder.Services.AddSingleton<RedisTenantContext>(sp => new RedisTenantContext("default", ""));

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();
app.UseCors("AllowAngularDev");
app.MapOpenApi();
app.UseHttpsRedirection();
app.MapCacheEndpoints();
app.Run();