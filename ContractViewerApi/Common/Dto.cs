namespace ContractViewerApi;

public class ContractSummary
{
    public required string ContractId { get; init; }
    public required string Role { get; init; }
    public required string RegistrationNumber { get; init; }

}

public class ContractDetails
{
    public required string ContractId { get; init; }
    public required string Role { get; init; }
    public required string RegistrationNumber { get; init; }
}