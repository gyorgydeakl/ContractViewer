using Common;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
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