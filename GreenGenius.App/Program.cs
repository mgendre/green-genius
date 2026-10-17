using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Beds;
using GreenGenius.Api.Features.Gardens;
using GreenGenius.Api.Features.Plants;
using GreenGenius.App.Extensions;
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
app.WithBedsApi();
app.MapHealthChecks(RouteConstants.Health);

if (!app.Environment.IsOpenApiGeneration())
{
    await app.MigrateAsync();

    if (app.Configuration.GetValue("PlantsImporterEnabled", true))
    {
        await app.ImportDataAsync();
    }
}

await app.RunAsync();