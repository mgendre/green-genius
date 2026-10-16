using System.Net;
using System.Text.Json;
using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Enums;
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
        plants.First().Family.ShouldNotBe(PlantFamily.Other);
        plants.First().Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task ListPlants_ShouldSerializeEnumsAsStrings()
    {
        await ExecuteInScopeAsync(async ctx =>
        {
            await ctx.PersistPlantAsync("Tomate", family: PlantFamily.Solanaceae, lifeCycle: PlantLifeCycle.Perennial);
        });

        var response = await CreateClient().GetAsync(RouteConstants.Plants);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("\"family\":\"Solanaceae\"");
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
    public async Task Import_WhenOnePlantFileIsRejectedByDb_ShouldStillImportTheOthers()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        await File.WriteAllTextAsync(Path.Combine(tempDirectory, "tomato.json"),
            JsonSerializer.Serialize(new
            {
                NameFr = "Tomate",
                DescriptionFr = "Plante nécessitant beaucoup d'eau et de chaleur",
                BinomialName = "Solanum lycopersicum",
                Emoji = "🍅",
                Family = "Solanaceae",
                LifeCycle = "Annual",
                Version = 99,
                Needs = new { Sunlight = "High", WaterNeed = "High", RootDepth = "Deep", SpacingRowCm = 100, SpacingPlantCm = 50, HeightCm = 120, SpreadCm = 50 },
                Traits = new { NitrogenFixer = false, DynamicAccumulator = false, PollinatorFriendly = true, DroughtTolerant = false }
            }));

        var badName = new string('A', 260);
        await File.WriteAllTextAsync(Path.Combine(tempDirectory, "bad.json"),
            JsonSerializer.Serialize(new
            {
                NameFr = badName,
                Family = "Solanaceae",
                LifeCycle = "Annual",
                Needs = new { Sunlight = "Low", WaterNeed = "Low", RootDepth = "Shallow", SpacingRowCm = 10, SpacingPlantCm = 10, HeightCm = 10, SpreadCm = 10 },
                Traits = new { NitrogenFixer = false, DynamicAccumulator = false, PollinatorFriendly = false, DroughtTolerant = false }
            }));

        Environment.SetEnvironmentVariable("GREENGENIUS_PLANTS_DIRECTORY", tempDirectory);

        var response = await CreateClient().GetAsync(RouteConstants.Plants);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await ExecuteInScopeAsync(async ctx =>
        {
            var tomato = await ctx.Plants
                .Where(p => p.Key == "tomato")
                .FirstOrDefaultAsync();
            tomato.ShouldNotBeNull();
            tomato.NameFr.ShouldBe("Tomate");
            tomato.Version.ShouldBe(99);

            var bad = await ctx.Plants
                .Where(p => p.Key == "bad")
                .AnyAsync();
            bad.ShouldBeFalse();
        });

        Directory.Delete(tempDirectory, true);
    }
}
