using GreenGenius.Api.Features.Gardens;
using GreenGenius.App.Extensions;
using GreenGenius.Common.Data.Extensions;
using JetBrains.Annotations;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.WithGardensApi();

app.UseExceptionHandler();
app.MapHealthChecks("/health");

await app.MigrateAsync();

await app.RunAsync();

// Required for integration tests
#pragma warning disable ASP0027
namespace GreenGenius.App
{
    // ReSharper disable once PartialTypeWithSinglePart
    [UsedImplicitly]
    public partial class Program;
}
#pragma warning restore ASP0027
