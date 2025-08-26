using System.Net;
using Common;
using ContractDetailsApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));
builder.Services.AddDbContext<ContractDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

var app = builder.Build();
app.MapOpenApi();

app.UseHttpsRedirection();
app.MapGet("contract/{contractId}", (string contractId, ContractDbContext db) =>
{
    Thread.Sleep(TimeSpan.FromSeconds(1)); // to simulate slow access to db

    return db
        .Set<Contract>()
        .First(c => c.ContractId == contractId)
        .ToDetails();
});

app.Run();