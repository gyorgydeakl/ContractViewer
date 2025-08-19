using System.Net;
using DocumentApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, Connection.Port));

var app = builder.Build();
app.MapOpenApi();

app.Run();
