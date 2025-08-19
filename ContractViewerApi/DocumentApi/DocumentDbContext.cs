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
}