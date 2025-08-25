using System.Net;
using Common;
using ContractListApi;
using Microsoft.AspNetCore.Mvc;
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

app.MapPost("user/{userId}/contracts/random", (string userId, [FromBody] GenerateContractRequest req, ContractDbContext db) =>
{
    var contracts = Contract.FakerForUser(userId).Generate(req.Count);
    db.Set<Contract>().AddRange(contracts);
    db.SaveChanges();
    return contracts.Select(c => c.ToSummary()).ToList();
});

app.MapGet("user/{userId}/contracts", (string userId, ContractDbContext db) =>
{
    Thread.Sleep(TimeSpan.FromSeconds(1));
    return db
        .Set<Contract>()
        .Where(c => c.UserId == userId)
        .Select(c => c.ToSummary())
        .ToList();
});

app.Run();