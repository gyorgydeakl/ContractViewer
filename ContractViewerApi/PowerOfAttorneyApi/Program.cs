using System.Net;
using PowerOfAttorneyApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));
builder.Services.AddDbContext<PoaDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});
var app = builder.Build();
app.MapOpenApi();

app.MapPost("poas/generate", ([FromBody] GeneratePoaRequest req, PoaDbContext db) =>
{
    var poas = PowerOfAttorney.FakerFor(req.ContractIds, req.UserIds).Generate(req.Count);
    db.Set<PowerOfAttorney>().AddRange(poas);
    db.SaveChanges();
    return poas.Select(d => d.ToDto()).ToList();
});

app.MapGet("poas", ([FromQuery] string? grantorId, [FromQuery] string? delegateId, PoaDbContext db) =>
{
    Thread.Sleep(TimeSpan.FromSeconds(1)); // to simulate slow access to db
    return db.Set<PowerOfAttorney>()
        .Where(p => p.GrantorUserId == grantorId || p.DelegateUserId == delegateId)
        .Select(p => p.ToDto())
        .ToList();
});
app.Run();


public record GeneratePoaRequest(string[] ContractIds, string[] UserIds, int Count);