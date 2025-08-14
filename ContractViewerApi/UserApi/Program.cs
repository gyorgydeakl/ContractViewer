using System.Net;
using Common;
using Microsoft.AspNetCore.Mvc;
using UserApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));

var app = builder.Build();
app.MapOpenApi();

app.MapGet("login", ([FromQuery] string username, [FromQuery] string password) =>
{
    var user = User.GetByUsername(username);
    return user?.Password == password ? TypedResults.Ok(user.UserId) : Results.Unauthorized();
});

app.MapGet("users", () => User.AllUsers
    .Select(u => u.ToSummary())
    .ToDictionary(u => u.Username, u => u.UserId));

app.Run();


