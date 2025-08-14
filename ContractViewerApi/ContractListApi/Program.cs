using System.Net;
using Common;
using ContractListApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));

var app = builder.Build();
app.MapOpenApi();
app.MapGet("user/{userId}/contracts", (string userId) => 
    User.GetByUserId(userId) is { } user ?
        Results.Ok(user.Contracts.Select(u => u.ToSummary())) : 
        Results.NotFound());

app.Run();
  