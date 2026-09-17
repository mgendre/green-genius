using GreenGenius.App.Extensions;
using GreenGenius.Common.Domain.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.UseHttpsRedirection();

await app.MigrateAsync();

await app.RunAsync();

// Required for integration tests
#pragma warning disable ASP0027
public partial class Program { }
#pragma warning restore ASP0027
