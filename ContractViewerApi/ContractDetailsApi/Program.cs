using System.Net;
using Common;
using ContractDetailsApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));
builder.Services.AddDbContext<ContractDetailsDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

var app = builder.Build();
app.MapOpenApi();

app.UseHttpsRedirection();
app.MapGet("contract/{contractId}", (string contractId) =>
        User.GetByContractId(contractId) is { } contract ?
            Results.Ok(contract.ToDetailed()) :
            Results.NotFound())
    .WithName("GetWeatherForecast");

app.Run();