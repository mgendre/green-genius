using System.Net;
using System.Text.Json;
using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using GreenGenius.App.IntegrationTests.Mocks;
using GreenGenius.Common.Data;
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
        await ExecuteInScopeAsync(async ctx => { await ctx.PersistPlantAsync(); });

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
        await ExecuteInScopeAsync(async ctx => { persisted = await ctx.PersistPlantAsync(); });

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
    public async Task GetPlant_ShouldSerializeFrostToleranceAndNotTheRenamedFields()
    {
        Plant persisted = null!;
        await ExecuteInScopeAsync(async ctx => { persisted = await ctx.PersistPlantAsync(frostToleranceC: -6); });

        var response = await CreateClient().GetAsync(GetPlantUrl(persisted.Id));
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldContain("\"frostToleranceC\":-6");
        body.ShouldNotContain("daysToMaturity");
        body.ShouldNotContain("temperatureMinC");
    }

    [Fact]
    public async Task GetPlant_WhenUnknownPlant_ShouldBeNotFound()
    {
        var response = await CreateClient().GetAsync(GetPlantUrl(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListPlantAssociations_ShouldReturnBothPlantsSortedByOtherPlantNameFr()
    {
        var targetId = Guid.Empty;
        var zucchiniId = Guid.Empty;
        var garlicId = Guid.Empty;
        await ExecuteInScopeAsync(async ctx =>
        {
            var target = await ctx.PersistPlantAsync();
            targetId = target.Id;
            var zucchini = await ctx.PersistPlantAsync("Zucchini", family: PlantFamily.Cucurbitaceae);
            zucchiniId = zucchini.Id;
            var garlic = await ctx.PersistPlantAsync("Ail", family: PlantFamily.Amaryllidaceae);
            garlicId = garlic.Id;

            await ctx.PersistPlantAssociationAsync(target, zucchini, noteFr: "note-zucchini");
            await ctx.PersistPlantAssociationAsync(garlic, target,
                effect: Effect.Antagonistic, mechanisms: [Mechanism.SharedPestOrDisease],
                evidence: Evidence.Limited, noteFr: "note-garlic");
        });

        var associations = await CreateClient().GetAndReadList<PlantAssociationDto>(GetAssociationsUrl(targetId));

        associations.Count.ShouldBe(2);
        associations.Select(a => a.PlantA.Id == targetId ? a.PlantB.NameFr : a.PlantA.NameFr)
            .ToList().ShouldBe(["Ail", "Zucchini"]);

        var first = associations.First();
        first.PlantA.Id.ShouldBe(garlicId);
        first.PlantA.NameFr.ShouldBe("Ail");
        first.PlantA.Family.ShouldBe(PlantFamily.Amaryllidaceae);
        first.PlantB.Id.ShouldBe(targetId);
        first.PlantB.NameFr.ShouldBe("Tomate");
        first.Effect.ShouldBe(Effect.Antagonistic);
        first.Mechanisms.ShouldBe([Mechanism.SharedPestOrDisease]);
        first.Evidence.ShouldBe(Evidence.Limited);
        first.NoteFr.ShouldBe("note-garlic");

        var second = associations[1];
        second.PlantA.Id.ShouldBe(targetId);
        second.PlantA.NameFr.ShouldBe("Tomate");
        second.PlantB.Id.ShouldBe(zucchiniId);
        second.PlantB.Family.ShouldBe(PlantFamily.Cucurbitaceae);
        second.Effect.ShouldBe(Effect.Beneficial);
        second.Evidence.ShouldBe(Evidence.Established);
        second.NoteFr.ShouldBe("note-zucchini");
    }

    [Fact]
    public async Task ListPlantAssociations_ShouldListMechanismsSortedByName()
    {
        var targetId = Guid.Empty;
        await ExecuteInScopeAsync(async ctx =>
        {
            var target = await ctx.PersistPlantAsync();
            targetId = target.Id;
            var carrot = await ctx.PersistPlantAsync();
            await ctx.PersistPlantAssociationAsync(target, carrot,
                mechanisms: [Mechanism.PestDeterrent, Mechanism.Allelopathy, Mechanism.GroundCover]);
        });

        var associations = await CreateClient().GetAndReadList<PlantAssociationDto>(GetAssociationsUrl(targetId));

        var first = associations.Single();
        first.Mechanisms.ShouldBe([Mechanism.Allelopathy, Mechanism.GroundCover, Mechanism.PestDeterrent]);
    }

    [Fact]
    public async Task ListPlantAssociations_WhenNoAssociation_ShouldReturnEmptyList()
    {
        var targetId = Guid.Empty;
        await ExecuteInScopeAsync(async ctx =>
        {
            var target = await ctx.PersistPlantAsync();
            targetId = target.Id;
        });

        var response = await CreateClient().GetAsync(GetAssociationsUrl(targetId));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var associations = await CreateClient().GetAndReadList<PlantAssociationDto>(GetAssociationsUrl(targetId));

        associations.ShouldBeEmpty();
    }

    [Fact]
    public async Task ListPlantAssociations_WhenUnknownPlant_ShouldBeNotFound()
    {
        var response = await CreateClient().GetAsync(GetAssociationsUrl(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static string GetPlantUrl(Guid id)
    {
        return RouteConstants.Plants + "/" + id;
    }

    private static string GetAssociationsUrl(Guid id)
    {
        return RouteConstants.Plants + "/" + id + "/associations";
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
    public async Task Import_WhenAppStarts_ShouldImportOneRowPerDataFile()
    {
        var plantFiles = Directory
            .GetFiles(Path.Combine(AppContext.BaseDirectory, "Data", "Plants"), "*.json")
            .Length;
        var associationFiles = Directory
            .GetFiles(Path.Combine(AppContext.BaseDirectory, "Data", "Associations"), "*.json")
            .Length;

        plantFiles.ShouldNotBe(0);
        associationFiles.ShouldNotBe(0);

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.Plants.CountAsync()).ShouldBe(plantFiles);
            (await ctx.PlantAssociations.CountAsync()).ShouldBe(associationFiles);
        });
    }

    [Fact]
    public async Task Import_WhenOnePlantFileIsRejectedByDb_ShouldFailTheWholeImport()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        await WritePlantFileAsync(tempDirectory, "tomato", version: 99);

        var badName = new string('A', 260);
        await WritePlantFileAsync(tempDirectory, "bad", nameFr: badName);

        Environment.SetEnvironmentVariable("GREENGENIUS_PLANTS_DIRECTORY", tempDirectory);

        await Should.ThrowAsync<DbUpdateException>(() => CreateClient().GetAsync(RouteConstants.Plants));

        Environment.SetEnvironmentVariable("GREENGENIUS_PLANTS_DIRECTORY", null);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(Fixture.GetPostgresContainerConnectionString())
            .Options;
        await using var db = new ApplicationDbContext(options, new CurrentUserMock());
        (await db.Plants.CountAsync()).ShouldBe(0);

        Directory.Delete(tempDirectory, true);
    }

    private static Task WritePlantFileAsync(string directory, string key, string nameFr = "Tomate", int version = 1)
    {
        var plant = new
        {
            NameFr = nameFr,
            DescriptionFr = "Plante nécessitant beaucoup d'eau et de chaleur",
            BinomialName = "Solanum lycopersicum",
            Emoji = "🍅",
            Family = "Solanaceae",
            LifeCycle = "Annual",
            Version = version,
            Needs = new
            {
                Sunlight = "High", WaterNeed = "High", RootDepth = "Deep", SpacingRowCm = 100, SpacingPlantCm = 50,
                HeightCm = 120, SpreadCm = 50
            },
            Traits = new
            {
                NitrogenFixer = false, DynamicAccumulator = false, PollinatorFriendly = true, DroughtTolerant = false
            }
        };

        return File.WriteAllTextAsync(Path.Combine(directory, key + ".json"), JsonSerializer.Serialize(plant));
    }
}