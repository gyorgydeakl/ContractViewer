using Bogus;
using Microsoft.EntityFrameworkCore;

namespace PowerOfAttorneyApi;

public class PoaDbContext(DbContextOptions<PoaDbContext> options): DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PowerOfAttorney>(db =>
        {
            db.HasKey(p => p.Id);
        });
    }
}

public class PowerOfAttorney
{
    public string Id { get; set; }

    public required string ContractId { get; set; }
    public required string GrantorUserId { get; set; }
    public required string DelegateUserId { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }

    // Metadata
    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByUserId { get; set; }

    public bool IsActive => !RevokedAt.HasValue && 
                            (!ValidUntil.HasValue || ValidUntil > DateTime.UtcNow);

    public string Notes { get; set; } = "";
    public string? ReferenceNumber { get; set; }

    // Audit
    public string? LastModifiedByUserId { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public PowerOfAttorneyDto ToDto()
    {
        return new PowerOfAttorneyDto()
        {
            Id = Id,
            ContractId = ContractId,
            GrantorUserId = GrantorUserId,
            DelegateUserId = DelegateUserId,
            ValidFrom = ValidFrom,
            ValidUntil = ValidUntil,
            CreatedByUserId = CreatedByUserId,
            CreatedAt = CreatedAt,
            RevokedAt = RevokedAt,
            RevokedByUserId = RevokedByUserId,
            Notes = Notes,
            ReferenceNumber = ReferenceNumber,
            LastModifiedByUserId = LastModifiedByUserId,
            LastModifiedAt = LastModifiedAt,
            IsActive = IsActive
        };
    }
    
    public static Faker<PowerOfAttorney> FakerFor(string[] contractIds, string[] userIds)
    {
        return new Faker<PowerOfAttorney>()
            .RuleFor(p => p.Id, f => Guid.NewGuid().ToString("N"))
            .RuleFor(p => p.ContractId, f => f.PickRandom(contractIds))
            .RuleFor(p => p.GrantorUserId, f => f.PickRandom(userIds))
            .RuleFor(p => p.DelegateUserId, (f, p) =>
            {
                // Ensure delegate is not the same as grantor
                var delegateId = f.PickRandom(userIds);
                while (delegateId == p.GrantorUserId)
                {
                    delegateId = f.PickRandom(userIds);
                }

                return delegateId;
            })
            .RuleFor(p => p.ValidFrom, f => f.Date.PastOffset(1).DateTime)
            .RuleFor(p => p.ValidUntil, (f, p) => f.Random.Bool(0.7f) ? p.ValidFrom?.AddMonths(f.Random.Int(1, 12)) : null)
            .RuleFor(p => p.CreatedByUserId, f => f.PickRandom(userIds))
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.RevokedAt, f => f.Random.Bool(0.2f) ? f.Date.Recent(30) : null)
            .RuleFor(p => p.RevokedByUserId, (f, p) => p.RevokedAt.HasValue ? f.PickRandom(userIds) : null)
            .RuleFor(p => p.Notes, f => f.Lorem.Sentence())
            .RuleFor(p => p.ReferenceNumber, f => f.Random.AlphaNumeric(10).ToUpper())
            .RuleFor(p => p.LastModifiedByUserId, f => f.Random.Bool(0.5f) ? f.PickRandom(userIds) : null)
            .RuleFor(p => p.LastModifiedAt, (f, p) => p.LastModifiedByUserId != null ? f.Date.Recent(15) : null);
    }
}
public class PowerOfAttorneyDto
{
    public required string Id { get; set; }

    public required string ContractId { get; set; }
    public required string GrantorUserId { get; set; }
    public required string DelegateUserId { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }
    public string? RevokedByUserId { get; set; }

    public string Notes { get; set; } = "";
    public string? ReferenceNumber { get; set; }

    public string? LastModifiedByUserId { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public bool IsActive { get; set; }
}