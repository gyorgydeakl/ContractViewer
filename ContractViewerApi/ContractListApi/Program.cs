using Common;

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

app.MapGet("user/{userId}/contracts", (string userId) => 
    User.GetByUserId(userId) is { } user ?
        Results.Ok(user.Contracts) : 
        Results.NotFound());

app.Run();
  