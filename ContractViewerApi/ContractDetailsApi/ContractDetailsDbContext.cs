using Microsoft.EntityFrameworkCore;

namespace ContractDetailsApi;

public class ContractDetailsDbContext(DbContextOptions<ContractDetailsDbContext> options) : DbContext(options)
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
}