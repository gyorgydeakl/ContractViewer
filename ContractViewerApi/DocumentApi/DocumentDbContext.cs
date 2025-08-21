using Bogus;
using Microsoft.EntityFrameworkCore;

namespace DocumentApi;

public class DocumentDbContext(DbContextOptions<DocumentDbContext> options): DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Document>(db =>
        {
            db.HasKey(d => d.DocumentId);
        });
    }
}

public class Document
{
    public required string ContractId { get; init; }
    public required string DocumentId { get; init; }
    public required string Subject { get; init; }
    public required DateTime Date { get; init; }
    
    public static Faker<Document> FakerFor(string[] contractIds)
    {
        var random = new Random();
        return new Faker<Document>()
            .RuleFor(d => d.ContractId, _ => contractIds[random.Next(contractIds.Length)])
            .RuleFor(d => d.DocumentId, f => Guid.NewGuid().ToString("N"))
            .RuleFor(d => d.Subject, f => f.Lorem.Sentence())
            .RuleFor(d => d.Date, f => f.Date.Past());
    }
    public DocumentDto ToDto() => new()
    {
        ContractId = ContractId,
        DocumentId = DocumentId,
        Subject = Subject,
        Date = Date
    };
}

public class DocumentDto
{
    public string ContractId { get; init; }
    public string DocumentId { get; init; }
    public string Subject { get; init; }
    public DateTime Date { get; init; }
}