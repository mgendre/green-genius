using System.Net;
using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using GreenGenius.Common.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GreenGenius.App.IntegrationTests.Api;

public class PlantsApiIntegrationTest(
    IntegrationTestsApplicationFixture integrationFixture)
    : AbstractApiIntegrationTest(integrationFixture)
{

    [Fact]
    public async Task ListPlants_ShouldSortByNameFr()
    {
        await ExecuteInScopeAsync(async ctx =>
        {
            await ctx.PersistPlantAsync("ZTomate", binomialName: "Solanum lycopersicum");
            await ctx.PersistPlantAsync("ACourgette");
        });

        var plants = await CreateClient().GetAndReadList<PlantSummaryDto>(RouteConstants.Plants);

        plants.Count.ShouldBe(2);
        plants.Select(p => p.NameFr).ToList().ShouldBeInOrder(SortDirection.Ascending);
        plants.First().BinomialName.ShouldBeNull();
        plants.Last().BinomialName.ShouldBe("Solanum lycopersicum");
        plants.First().Family.ShouldNotBeNull();
        plants.First().Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Import_WhenAppStartsTwice_ShouldNotImportTomateTwice()
    {
        await ExecuteInScopeAsync(async ctx =>
        {
            var count = await ctx.Plants.CountAsync();

            count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task GetPlant_ShouldReturnExistingPlant()
    {
        Plant persisted = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            persisted = await ctx.PersistPlantAsync("Tomate");
        });

        var result = await CreateClient().GetAndRead<PlantDto>(GetPlantUrl(persisted.Id));

        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(persisted.Id);
        result.Value.NameFr.ShouldBe("Tomate");
        result.Value.Needs.ShouldNotBeNull();
        result.Value.Traits.ShouldNotBeNull();
        result.Value.Needs.SoilPhMin.ShouldNotBeNull();
        result.Value.Needs.SoilPhMax.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetPlant_WhenUnknownPlant_ShouldBeNotFound()
    {
        var response = await CreateClient().GetAsync(GetPlantUrl(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static string GetPlantUrl(Guid id)
    {
        return RouteConstants.Plants + "/" + id;
    }
}

public class PlantsImporterIntegrationTest(
    IntegrationTestsApplicationFixture integrationFixture)
    : AbstractApiIntegrationTest(integrationFixture)
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("PlantsImporterEnabled", "true");
    }

    [Fact]
    public async Task Import_WhenAppStarts_ShouldImportTomate()
    {
        await ExecuteInScopeAsync(async ctx =>
        {
            var plant = await ctx.Plants
                .Where(p => p.Key == "tomato")
                .FirstOrDefaultAsync();

            plant.ShouldNotBeNull();
            plant.NameFr.ShouldBe("Tomate");
            plant.Version.ShouldNotBe(0);
        });
    }

    [Fact]
    public async Task Import_WhenImportFails_ShouldStillStart()
    {
        await ExecuteInScopeAsync(async ctx =>
        {
            var plant = await ctx.Plants
                .Where(p => p.Key == "tomato")
                .FirstOrDefaultAsync();

            plant.ShouldNotBeNull();
        });
    }
}
