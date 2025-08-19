namespace Common;

public class Contract
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
    public required string RegistrationNumber { get; init; }
    
    public required List<Document> Documents { get; init; }
    

    public ContractDetails ToDetailed() => new()
    {
        ContractId = Id,
        Role = Role,
        RegistrationNumber = RegistrationNumber,
    };
}

public class ContractDetails
{
    public required string ContractId { get; init; }
    public required string Role { get; init; }
    public required string RegistrationNumber { get; init; }
}