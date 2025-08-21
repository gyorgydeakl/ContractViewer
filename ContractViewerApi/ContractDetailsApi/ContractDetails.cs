using Common;

namespace ContractDetailsApi;

public static class ContractExtensions
{
    public static ContractDetails ToDetails(this Contract contract) => new()
    {
        ContractId = contract.ContractId,
        Role = contract.Role,
        RegistrationNumber = contract.RegistrationNumber,
        Name = contract.Name,
        PolicyNumber = contract.PolicyNumber,
        InsuranceName = contract.InsuranceName,
        InsuranceType = contract.InsuranceType,
        CommunicationChannel = contract.CommunicationChannel,
        BillingMethod = contract.BillingMethod,
        StartDate = contract.StartDate,
        Anniversary = contract.Anniversary,
        EndDate = contract.EndDate,
        HolderName = contract.HolderName,
        BirthPlace = contract.BirthPlace,
        BirthDate = contract.BirthDate,
        MothersName = contract.MothersName,
        ResidenceAddress = contract.ResidenceAddress,
        MailingAddress = contract.MailingAddress,
        PhoneNumber = contract.PhoneNumber,
        Email = contract.Email,
        AnnualPremium = contract.AnnualPremium,
        InstallmentPremium = contract.InstallmentPremium,
        PaymentFrequency = contract.PaymentFrequency,
        PaymentMethod = contract.PaymentMethod,
        LastPaymentDate = contract.LastPaymentDate,
        LastPaidAmount = contract.LastPaidAmount,
        NextDueDate = contract.NextDueDate,
        AmountDue = contract.AmountDue,
        UserId = contract.UserId
    };
}
public class ContractDetails
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
}