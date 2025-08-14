 using ContractViewerApi;
 using Scalar.AspNetCore;
 using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("RedisCache")!);
    options.AllowAdmin = true;
    return ConnectionMultiplexer.Connect(options);
});
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors("AllowAngularDev");
app.MapAppEndpoints();
app.Run();