 using ContractViewerApi;
 using Microsoft.AspNetCore.DataProtection;
 using Microsoft.AspNetCore.Identity;
 using Microsoft.OpenApi.Models;
 using Scalar.AspNetCore;
 using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Components ??= new OpenApiComponents();
        doc.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "JWT in the form: Bearer {token}"
        };

        // Global security requirement (applies to ops unless [AllowAnonymous])
        doc.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        doc.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("C:/shared-keys")) 
    .SetApplicationName("ContractViewerAuth");
builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultChallengeScheme = IdentityConstants.BearerScheme;
        opt.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
    })
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("RedisCache")!);
    options.AllowAdmin = true;
    return ConnectionMultiplexer.Connect(options);
});

builder.Services.AddHttpClientForApi(Apis.ContractDetails);
builder.Services.AddHttpClientForApi(Apis.ContractList);
builder.Services.AddHttpClientForApi(Apis.User);
builder.Services.AddHttpClientForApi(Apis.Document);

builder.Services.AddCors(options => 
    options.AddPolicy("AllowAngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapAppEndpoints();
app.Run();