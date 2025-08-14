namespace Common;

public class User
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string UserId { get; init; }
    public required List<Contract> Contracts { get; init; }
    
    public static User? GetByUsername(string username)
    {
        return AllUsers.FirstOrDefault(u => u.Username == username);
    }
    public static User? GetByUserId(string id)
    {
        return AllUsers.FirstOrDefault(u => u.UserId == id);
    }
    public static Contract? GetByContractId(string id)
    {
        return AllUsers.SelectMany(u => u.Contracts).FirstOrDefault(c => c.Id == id);
    }
    
    public static readonly List<User> AllUsers =
    [
        new()
        {
            Username = "admin",
            Password = "admin",
            UserId = "admin_userid",
            Contracts =
            [
                new Contract
                {
                    Id = "contract_123",
                    Name = "Contract 123",
                    Documents =
                    [
                        new Document
                        {
                            Id = "doc_123",
                            Name = "Document 123"
                        }
                    ]
                }
            ]
        },
        new()
        {
            Username = "john_doe",
            Password = "pass123",
            UserId = "user_001",
            Contracts =
            [
                new Contract
                {
                    Id = "contract_456",
                    Name = "Website Redesign",
                    Documents =
                    [
                        new Document { Id = "doc_456", Name = "Wireframe.pdf" },
                        new Document { Id = "doc_457", Name = "Requirements.docx" }
                    ]
                },
                new Contract
                {
                    Id = "contract_789",
                    Name = "App Development",
                    Documents =
                    [
                        new Document { Id = "doc_458", Name = "UI_Mockup.png" }
                    ]
                }
            ]
        },
        new()
        {
            Username = "jane_smith",
            Password = "securepass",
            UserId = "user_002",
            Contracts =
            [
                new Contract
                {
                    Id = "contract_987",
                    Name = "Marketing Campaign",
                    Documents =
                    [
                        new Document { Id = "doc_900", Name = "Plan.pdf" },
                        new Document { Id = "doc_901", Name = "Budget.xlsx" }
                    ]
                }
            ]
        },
    ];
}