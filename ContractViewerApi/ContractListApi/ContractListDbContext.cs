using Bogus;
using Microsoft.EntityFrameworkCore;

namespace ContractListApi;

public class ContractListDbContext(DbContextOptions<ContractListDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Contract>(cb =>
        {
            cb.HasKey(c => c.ContractId);
        });
    }
}

public class Contract
{
    public required string UserId { get; init; }
    public required string ContractId { get; init; }
    public required string Name { get; init; }
    public required string RegistrationNumber { get; init; }
    public required string Role { get; init; }
    public string? PolicyNumber { get; init; }
    public string? InsuranceName { get; init; }
    public string? InsuranceType { get; init; }
    public string? CommunicationChannel { get; init; }
    public string? BillingMethod { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? Anniversary { get; init; }
    public DateOnly? EndDate { get; init; }

    // Holder
    public string? HolderName { get; init; }
    public string? BirthPlace { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? MothersName { get; init; }
    public string? ResidenceAddress { get; init; }
    public string? MailingAddress { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }

    // Payment
    public decimal? AnnualPremium { get; init; }
    public decimal? InstallmentPremium { get; init; }
    public string? PaymentFrequency { get; init; }
    public string? PaymentMethod { get; init; }
    public DateOnly? LastPaymentDate { get; init; }
    public decimal? LastPaidAmount { get; init; }
    public DateOnly? NextDueDate { get; init; }
    public decimal? AmountDue { get; init; }

    public ContractSummary ToSummary() => new()
    {
        ContractId = ContractId,
        Role = Role,
        RegistrationNumber = RegistrationNumber,
    };

    public static Faker<Contract> FakerForUser(string userId) =>
        new Faker<Contract>()
            .RuleFor(c => c.UserId, _ => userId)
            .RuleFor(c => c.ContractId, f => Guid.NewGuid().ToString("N"))
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.RegistrationNumber, f => f.Random.AlphaNumeric(10).ToUpper())
            .RuleFor(c => c.Role, f => f.PickRandom(new[] { "holder", "insured", "beneficiary" }))
            .RuleFor(c => c.PolicyNumber, f => f.Random.AlphaNumeric(12).ToUpper())
            .RuleFor(c => c.InsuranceName, f => f.Company.CompanyName())
            .RuleFor(c => c.InsuranceType, f => f.PickRandom("Life", "Health", "Property", "Auto"))
            .RuleFor(c => c.CommunicationChannel, f => f.PickRandom("Email", "Phone", "Mail"))
            .RuleFor(c => c.BillingMethod, f => f.PickRandom("Direct Debit", "Card", "Invoice"))
            .RuleFor(c => c.StartDate, f => DateOnly.FromDateTime(f.Date.Past(10)))
            .RuleFor(c => c.Anniversary, (f, c) => c.StartDate.HasValue ? c.StartDate.Value.AddYears(f.Random.Int(1, 10)) : null)
            .RuleFor(c => c.EndDate, f => f.Random.Bool(0.2f) ? DateOnly.FromDateTime(f.Date.Future(5)) : null)
            .RuleFor(c => c.HolderName, f => f.Name.FullName())
            .RuleFor(c => c.BirthPlace, f => f.Address.City())
            .RuleFor(c => c.BirthDate, f => DateOnly.FromDateTime(f.Date.Past(70, DateTime.Now.AddYears(-18))))
            .RuleFor(c => c.MothersName, f => f.Name.FullName())
            .RuleFor(c => c.ResidenceAddress, f => f.Address.FullAddress())
            .RuleFor(c => c.MailingAddress, f => f.Address.FullAddress())
            .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.AnnualPremium, f => f.Finance.Amount(100, 5000))
            .RuleFor(c => c.InstallmentPremium, (f, c) => c.AnnualPremium.HasValue ? Math.Round(c.AnnualPremium.Value / 12m, 2) : null)
            .RuleFor(c => c.PaymentFrequency, f => f.PickRandom("Monthly", "Quarterly", "Annual"))
            .RuleFor(c => c.PaymentMethod, f => f.PickRandom("Card", "Transfer", "Cash"))
            .RuleFor(c => c.LastPaymentDate, f => DateOnly.FromDateTime(f.Date.Recent(120)))
            .RuleFor(c => c.LastPaidAmount, f => f.Finance.Amount(50, 500))
            .RuleFor(c => c.NextDueDate, f => DateOnly.FromDateTime(f.Date.Future(90)))
            .RuleFor(c => c.AmountDue, f => f.Finance.Amount(0, 1000));
}

public class ContractSummary
{
    public required string ContractId { get; init; }
    public required string Role { get; init; }
    public required string RegistrationNumber { get; init; }
}
public record GenerateContractRequest(int Count);