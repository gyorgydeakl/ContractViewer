namespace Common;

public class Contract
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    
    public required List<Document> Documents { get; init; } = [
        
    ];
}