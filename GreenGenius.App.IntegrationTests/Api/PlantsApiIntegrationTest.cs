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

    [Fact]
    public async Task ListPlantAssociations_ShouldReturnOtherPlantsSortedByNameFr()
    {
        var targetId = Guid.Empty;
        await ExecuteInScopeAsync(async ctx =>
        {
            var target = await ctx.PersistPlantAsync("Tomate");
            targetId = target.Id;
            var zucchini = await ctx.PersistPlantAsync("Zucchini", family: PlantFamily.Cucurbitaceae);
            var garlic = await ctx.PersistPlantAsync("Ail", family: PlantFamily.Amaryllidaceae);

            await ctx.PersistPlantAssociationAsync(target, zucchini,
                polarity: Polarity.Beneficial, mechanism: Mechanism.PestDeterrent,
                evidence: Evidence.Established, noteFr: "note-zucchini");
            await ctx.PersistPlantAssociationAsync(garlic, target,
                polarity: Polarity.Antagonistic, mechanism: Mechanism.SharedPestOrDisease,
                evidence: Evidence.Limited, noteFr: "note-garlic");
        });

        var associations = await CreateClient().GetAndReadList<PlantAssociationDto>(GetAssociationsUrl(targetId));

        associations.Count.ShouldBe(2);
        associations.Select(a => a.Plant.NameFr).ToList().ShouldBe(["Ail", "Zucchini"]);

        var first = associations.First();
        first.Plant.NameFr.ShouldBe("Ail");
        first.Plant.Family.ShouldBe(PlantFamily.Amaryllidaceae);
        first.Polarity.ShouldBe(Polarity.Antagonistic);
        first.Mechanism.ShouldBe(Mechanism.SharedPestOrDisease);
        first.Evidence.ShouldBe(Evidence.Limited);
        first.NoteFr.ShouldBe("note-garlic");
    }

    [Fact]
    public async Task ListPlantAssociations_WhenNoAssociation_ShouldReturnEmptyList()
    {
        var targetId = Guid.Empty;
        await ExecuteInScopeAsync(async ctx =>
        {
            var target = await ctx.PersistPlantAsync("Tomate");
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

    [Fact]
    public async Task Import_WhenAppStarts_ShouldImportPotatoCarrotAndOnion()
    {
        var plants = new Dictionary<string, Plant>();
        await ExecuteInScopeAsync(async ctx =>
        {
            var rows = await ctx.Plants
                .Include(p => p.Needs)
                .Include(p => p.Traits)
                .Where(p => p.Key == "potato" || p.Key == "carrot" || p.Key == "onion")
                .ToListAsync();
            foreach (var plant in rows)
            {
                plants[plant.Key!] = plant;
            }
        });

        plants.Count.ShouldBe(3);

        var potato = plants["potato"];
        potato.NameFr.ShouldBe("Pomme de terre");
        potato.BinomialName.ShouldBe("Solanum tuberosum");
        potato.Emoji.ShouldBe("🥔");
        potato.Family.ShouldBe(PlantFamily.Solanaceae);
        potato.LifeCycle.ShouldBe(PlantLifeCycle.Annual);
        potato.Version.ShouldBe(1);
        potato.Needs.RootDepth.ShouldBe(RootDepthLevel.Medium);
        potato.Needs.SoilPhMin.ShouldBe(5.0m);
        potato.Needs.SoilPhMax.ShouldBe(6.5m);
        potato.Needs.SpacingRowCm.ShouldBe(70);
        potato.Needs.SpacingPlantCm.ShouldBe(35);
        potato.Traits.PollinatorFriendly.ShouldBeFalse();

        var carrot = plants["carrot"];
        carrot.NameFr.ShouldBe("Carotte");
        carrot.BinomialName.ShouldBe("Daucus carota subsp. sativus");
        carrot.Emoji.ShouldBe("🥕");
        carrot.Family.ShouldBe(PlantFamily.Apiaceae);
        carrot.LifeCycle.ShouldBe(PlantLifeCycle.Biennial);
        carrot.Version.ShouldBe(1);
        carrot.Needs.RootDepth.ShouldBe(RootDepthLevel.Deep);
        carrot.Needs.SoilPhMin.ShouldBe(6.0m);
        carrot.Needs.SoilPhMax.ShouldBe(7.0m);
        carrot.Needs.SpacingPlantCm.ShouldBe(5);
        carrot.Traits.PollinatorFriendly.ShouldBeFalse();

        var onion = plants["onion"];
        onion.NameFr.ShouldBe("Oignon");
        onion.BinomialName.ShouldBe("Allium cepa");
        onion.Emoji.ShouldBe("🧅");
        onion.Family.ShouldBe(PlantFamily.Amaryllidaceae);
        onion.LifeCycle.ShouldBe(PlantLifeCycle.Biennial);
        onion.Version.ShouldBe(1);
        onion.Needs.RootDepth.ShouldBe(RootDepthLevel.Shallow);
        onion.Needs.SoilPhMin.ShouldBe(6.0m);
        onion.Needs.SoilPhMax.ShouldBe(7.0m);
        onion.Needs.SpacingPlantCm.ShouldBe(10);
        onion.Traits.PollinatorFriendly.ShouldBeFalse();
    }

    [Theory]
    [InlineData("tomato", "Pomme de terre", Polarity.Antagonistic, Mechanism.SharedPestOrDisease,
        Evidence.Established,
        "La tomate et la pomme de terre partagent le mildiou, qui passe facilement de l’une à l’autre : cultivez-les dans des planches éloignées.")]
    [InlineData("carrot", "Oignon", Polarity.Beneficial, Mechanism.PestDeterrent,
        Evidence.Limited,
        "L’oignon masque l’odeur de la carotte à la mouche de la carotte, mais seulement avec environ quatre rangs d’oignon pour un rang de carotte et tant que l’oignon est en pleine feuille.")]
    public async Task ListPlantAssociations_WhenAppStarts_ShouldReturnSeedAssociation(
        string plantKey, string otherNameFr, Polarity polarity, Mechanism mechanism, Evidence evidence, string noteFr)
    {
        var plantId = await PlantIdAsync(plantKey);

        var associations = await CreateClient().GetAndReadList<PlantAssociationDto>(GetAssociationsUrl(plantId));

        associations.Count.ShouldBe(1);
        var association = associations.Single();
        association.Plant.NameFr.ShouldBe(otherNameFr);
        association.Polarity.ShouldBe(polarity);
        association.Mechanism.ShouldBe(mechanism);
        association.Evidence.ShouldBe(evidence);
        association.NoteFr.ShouldBe(noteFr);
    }

    private async Task<Guid> PlantIdAsync(string plantKey)
    {
        var plantId = Guid.Empty;
        await ExecuteInScopeAsync(async ctx =>
        {
            plantId = await ctx.Plants
                .Where(p => p.Key == plantKey)
                .Select(p => p.Id)
                .SingleAsync();
        });

        return plantId;
    }

    private static string GetAssociationsUrl(Guid id)
    {
        return RouteConstants.Plants + "/" + id + "/associations";
    }
}
