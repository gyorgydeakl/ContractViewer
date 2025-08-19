using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UserApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("C:/shared-keys")) 
    .SetApplicationName("ContractViewerAuth"); 

builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultChallengeScheme = IdentityConstants.BearerScheme;
        opt.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
    })
    .AddBearerToken(IdentityConstants.BearerScheme, options =>
    {
        // Log what the handler sees and why it fails (if it does)
        options.Events.OnMessageReceived = ctx =>
        {
            var raw = ctx.Request.Headers["Authorization"].ToString();
            Console.WriteLine($"[Bearer] OnMessageReceived Authorization header: '{raw}'");
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});
builder.Services
    .AddIdentity<User, IdentityRole<string>>()
    .AddRoles<IdentityRole<string>>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();
var app = builder.Build();
await IdentitySeeder.SeedAsync(app.Services);
app.MapOpenApi();
app.MapScalarApiReference();
app.UseAuthentication();
app.Use(async (ctx, next) =>
{
    // Log whether the request is authenticated right after authentication runs
    Console.WriteLine($"Authenticated: {ctx.User?.Identity?.IsAuthenticated == true}");
    await next();
});

app.UseAuthorization();
app.MapIdentityApi<User>();
app.MapGet("userId", (HttpContext ctx, UserDbContext db) =>
{
    Console.WriteLine("User ID GET");
    var email = ctx.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
    return db.Set<User>().First(u => u.Email == email).Id;
}).RequireAuthorization();
app.Run();


