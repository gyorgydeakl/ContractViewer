using Common;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapGet("login", ([FromQuery] string username, [FromQuery] string password) =>
{
    var user = User.GetByUsername(username);
    return user?.Password == password ? TypedResults.Ok(user.UserId) : Results.Unauthorized();
});

app.Run();


