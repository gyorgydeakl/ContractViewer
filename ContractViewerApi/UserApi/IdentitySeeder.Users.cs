namespace UserApi;

public static partial class IdentitySeeder
{
    private static readonly User[] Users =
    [
        new() { Id = "user-1", UserName = "alice", Email = "alice@example.com", EmailConfirmed = true },
        new() { Id = "user-2", UserName = "bob",   Email = "bob@example.com",   EmailConfirmed = true },
        new() { Id = "user-3", UserName = "carol", Email = "carol@example.com", EmailConfirmed = true },
        new() { Id = "user-4", UserName = "dave",  Email = "dave@example.com",  EmailConfirmed = true },
        new() { Id = "user-5", UserName = "eve",   Email = "eve@example.com",   EmailConfirmed = true }
    ];
}