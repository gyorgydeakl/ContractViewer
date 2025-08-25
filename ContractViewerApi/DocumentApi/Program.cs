using System.Net;
using DocumentApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));
builder.Services.AddDbContext<DocumentDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});
var app = builder.Build();
app.MapOpenApi();

app.MapPost("documents/generate", ([FromBody] GenerateDocumentRequest req, DocumentDbContext db) =>
{
    var documents = Document.FakerFor(req.ContractIds).Generate(req.Count);
    db.Set<Document>().AddRange(documents);
    db.SaveChanges();
    return documents.Select(d => d.ToDto()).ToList();
});

app.MapGet("documents", ([FromQuery] string contracts, DocumentDbContext db) =>
{
    Thread.Sleep(TimeSpan.FromSeconds(1));
    var contractIds = Uri.UnescapeDataString(contracts).Split(',');
    return db
        .Set<Document>()
        .Where(d => contractIds.Contains(d.ContractId))
        .Select(d => d.ToDto())
        .ToList();
});
app.Run();


public record GenerateDocumentRequest(string[] ContractIds, int Count);