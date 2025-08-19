using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserApi;

public class UserDbContext(DbContextOptions<UserDbContext> options) 
    : IdentityDbContext<User, IdentityRole<string>, string>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

public class User : IdentityUser;


public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        await db.Database.MigrateAsync();

        User[] users =
        [
            new() { Id = "user-1", UserName = "alice", Email = "alice@example.com", EmailConfirmed = true },
            new() { Id = "user-2", UserName = "bob",   Email = "bob@example.com",   EmailConfirmed = true },
            new() { Id = "user-3", UserName = "carol", Email = "carol@example.com", EmailConfirmed = true },
            new() { Id = "user-4", UserName = "dave",  Email = "dave@example.com",  EmailConfirmed = true },
            new() { Id = "user-5", UserName = "eve",   Email = "eve@example.com",   EmailConfirmed = true }
        ];

        foreach (var u in users)
        {
            u.NormalizedUserName = userMgr.NormalizeName(u.UserName!);
            u.NormalizedEmail = userMgr.NormalizeEmail(u.Email!);

            var existing = await userMgr.FindByIdAsync(u.Id) ?? await userMgr.FindByNameAsync(u.UserName!);
            if (existing is not null)
            {
                continue;
            }

            var res = await userMgr.CreateAsync(u, "Admin_123");
            if (res.Succeeded)
            {
                continue;
            }

            var errors = string.Join(", ", res.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new Exception($"Failed to create seed user {u.UserName}: {errors}");
        }
    }
}
