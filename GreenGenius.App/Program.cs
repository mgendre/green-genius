using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Gardens;
using GreenGenius.Api.Features.Plants;
using GreenGenius.App.Extensions;
using GreenGenius.App.Services;
using GreenGenius.Common.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.WithGardensApi();
app.WithPlantsApi();
app.MapHealthChecks(RouteConstants.Health);

if (!app.Environment.IsOpenApiGeneration())
{
    await app.MigrateAsync();

    if (app.Configuration["PlantsImporterEnabled"] != "false")
    {
        using var scope = app.Services.CreateScope();
        var importer = scope.ServiceProvider.GetRequiredService<PlantImporter>();
        await importer.ImportAsync();
    }
}

await app.RunAsync();
