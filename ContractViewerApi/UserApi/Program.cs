using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UserApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/shared-keys")) 
    .SetApplicationName("ContractViewerAuth");

builder.Services
    .AddIdentity<User, IdentityRole<string>>()
    .AddRoles<IdentityRole<string>>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();
builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultChallengeScheme = IdentityConstants.BearerScheme;
        opt.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
    })
    .AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorization();
builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});
var app = builder.Build();
await IdentitySeeder.SeedAsync(app.Services);
app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthentication();
app.UseAuthorization();
app.MapIdentityApi<User>();
app.MapGet("userId", (HttpContext ctx, UserDbContext db) =>
{
    Console.WriteLine("User ID GET");
    var email = ctx.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
    return db.Set<User>().First(u => u.Email == email).Id;
})
.RequireAuthorization();
app.Run();


