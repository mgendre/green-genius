using GreenGenius.App.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.UseHttpsRedirection();

await app.RunAsync();

// Required for integration tests
#pragma warning disable ASP0027
public partial class Program { }
#pragma warning restore ASP0027
