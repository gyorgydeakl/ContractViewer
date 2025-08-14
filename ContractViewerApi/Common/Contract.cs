namespace Common;

public class Contract
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
    public required string RegistrationNumber { get; init; }
    
    public required List<Document> Documents { get; init; }

    public ContractSummary ToSummary() => new()
    {
        ContractId = Id,
        Role = Role,
        RegistrationNumber = RegistrationNumber,
    };

    public ContractDetails ToDetailed() => new()
    {
        ContractId = Id,
        Role = Role,
        RegistrationNumber = RegistrationNumber,
    };
}

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