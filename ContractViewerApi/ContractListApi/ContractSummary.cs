using Common;

namespace ContractListApi;
public static class ContractExtensions
{
    public static ContractSummary ToSummary(this Contract contract) => new()
    {
        ContractId = contract.ContractId,
        Role = contract.Role,
        RegistrationNumber = contract.RegistrationNumber,
    };
}
public class ContractSummary
{
    public required string ContractId { get; init; }
    public required string Role { get; init; }
    public required string RegistrationNumber { get; init; }
}