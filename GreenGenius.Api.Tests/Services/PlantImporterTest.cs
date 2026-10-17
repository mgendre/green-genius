using GreenGenius.Api.Tests.Infra;
using GreenGenius.App.Services;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace GreenGenius.Api.Tests.Services;

internal static class PlantQuery
{
    extension(IQueryable<Plant> plants)
    {
        public Task<bool> ExistsAsync() => plants.AnyAsync();

        public Plant? FindPlant(string key)
            => plants.FirstOrDefault(p => p.Key == key);
    }
}

public class PlantImporterTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private readonly string _databaseName = Guid.NewGuid().ToString();

    private readonly string _directory;

    public PlantImporterTest()
    {
        _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_directory);
    }

    private ApplicationDbContext NewDbContext()
        => InMemoryDbContextFactory.Create(_databaseName, OwnerId);

    [Fact]
    public async Task ImportAsync_WhenMapping_ShouldMapEveryPlantField()
    {
        await WritePlantFileAsync(
            CreatePlant(descriptionFr: "Plante nécessitant beaucoup d'eau", frostToleranceC: -6), "tomato");

        var db = NewDbContext();
        var importer = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);

        var count = await importer.ImportAsync();

        count.ShouldBe(1);

        var db2 = NewDbContext();
        var plant = db2.Plants
            .Include(p => p.Needs)
            .Include(p => p.Traits)
            .FindPlant("tomato");
        plant.ShouldNotBeNull();
        plant.NameFr.ShouldBe("Tomate");
        plant.DescriptionFr.ShouldBe("Plante nécessitant beaucoup d'eau");
        plant.BinomialName.ShouldBe("Solanum lycopersicum");
        plant.Family.ShouldBe(PlantFamily.Solanaceae);
        plant.LifeCycle.ShouldBe(PlantLifeCycle.Annual);
        plant.FrostToleranceC.ShouldBe(-6);
        plant.Version.ShouldBe(1);
        plant.Needs.ShouldNotBeNull();
        plant.Traits.ShouldNotBeNull();
    }

    [Fact]
    public async Task ImportAsync_WhenImportingSameFileTwice_ShouldKeepOnePlant()
    {
        await WritePlantFileAsync(CreatePlant());

        var db = NewDbContext();
        var importer = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);
        await importer.ImportAsync();
        await importer.ImportAsync();

        var db2 = NewDbContext();
        var plants = db2.Plants
            .Include(plant => plant.Needs)
            .Include(plant => plant.Traits)
            .ToList();
        plants.Count.ShouldBe(1);
        plants[0].Needs.ShouldNotBeNull();
        plants[0].Traits.ShouldNotBeNull();
    }

    [Fact]
    public async Task ImportAsync_WhenFileVersionGreaterThanStored_ShouldUpdate()
    {
        await WritePlantFileAsync(CreatePlant(descriptionFr: "Ancienne"));

        var db = NewDbContext();
        var first = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);
        await first.ImportAsync();

        await WritePlantFileAsync(CreatePlant(descriptionFr: "Nouvelle", version: 2));

        var second = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);
        await second.ImportAsync();

        var db2 = NewDbContext();
        var count = await db2.Plants.ExistsAsync();
        count.ShouldBe(true);
        db2.Plants.First().DescriptionFr.ShouldBe("Nouvelle");
    }

    [Fact]
    public async Task ImportAsync_WhenFileVersionNotGreaterThanStored_ShouldNotUpdate()
    {
        await WritePlantFileAsync(CreatePlant(descriptionFr: "Ancienne", version: 3));

        var db = NewDbContext();
        var first = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);
        await first.ImportAsync();

        await WritePlantFileAsync(CreatePlant(descriptionFr: "Plus ancienne", version: 2));

        var second = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);
        await second.ImportAsync();

        var db2 = NewDbContext();
        db2.Plants.First().DescriptionFr.ShouldBe("Ancienne");
    }

    [Fact]
    public async Task ImportAsync_WhenStoredVersionMissing_ShouldCountAsZero()
    {
        await WritePlantFileAsync(CreatePlant(descriptionFr: "Ancienne", version: 0));

        var db = NewDbContext();
        var first = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);
        await first.ImportAsync();
        db.Plants.First().Version.ShouldBe(0);

        await WritePlantFileAsync(CreatePlant(descriptionFr: "Nouvelle"));

        var second = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);
        await second.ImportAsync();

        var db2 = NewDbContext();
        db2.Plants.First().DescriptionFr.ShouldBe("Nouvelle");
    }

    [Fact]
    public async Task ImportAsync_WhenNeedsAndTraitsMissing_ShouldAlwaysCreateBoth()
    {
        await WritePlantFileAsync(CreatePlant(descriptionFr: null), "tomato");

        var db = NewDbContext();
        var importer = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);

        await importer.ImportAsync();

        var db2 = NewDbContext();
        var plant = db2.Plants
            .Include(p => p.Needs)
            .Include(p => p.Traits)
            .FindPlant("tomato");
        plant!.Needs.ShouldNotBeNull();
        plant.Traits.ShouldNotBeNull();
        plant.Needs.PlantId.ShouldBe(plant.Id);
        plant.Traits.PlantId.ShouldBe(plant.Id);
    }

    [Fact]
    public async Task ImportAsync_WhenReimportingWithHigherVersion_ShouldKeepOneOfEach()
    {
        await WritePlantFileAsync(CreatePlant(descriptionFr: "Ancienne", nitrogenFixer: true));

        var db = NewDbContext();
        var first = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);
        await first.ImportAsync();

        await WritePlantFileAsync(
            CreatePlant(descriptionFr: "Nouvelle", version: 2,
                sunlight: SunlightLevel.Medium, waterNeed: WaterNeedLevel.High, dynamicAccumulator: true),
            "tomato");

        var second = new PlantImporter(NewDbContext(), NullLogger<PlantImporter>.Instance, _directory);
        await second.ImportAsync();

        var db2 = NewDbContext();
        var plant = db2.Plants
            .Include(p => p.Needs)
            .Include(p => p.Traits)
            .Single(p => p.Key == "tomato");
        plant.Version.ShouldBe(2);
        plant.DescriptionFr.ShouldBe("Nouvelle");
        plant.Needs.Sunlight.ShouldBe(SunlightLevel.Medium);
        plant.Needs.WaterNeed.ShouldBe(WaterNeedLevel.High);
        plant.Traits.NitrogenFixer.ShouldBe(false);
        plant.Traits.DynamicAccumulator.ShouldBe(true);
    }

    [Fact]
    public async Task ImportAsync_WhenOnePlantFails_ShouldLogAndSaveTheOthers()
    {
        await WritePlantFileAsync(CreatePlant(descriptionFr: "Bonne"), "tomato");
        await PlantJsonWriter.WriteAsync(_directory, "broken", "{}");

        var db = NewDbContext();
        var importer = new PlantImporter(db, NullLogger<PlantImporter>.Instance, _directory);

        var count = await importer.ImportAsync();

        count.ShouldBe(1);

        var db2 = NewDbContext();
        var good = db2.Plants.FindPlant("tomato");
        good.ShouldNotBeNull();
        good.DescriptionFr.ShouldBe("Bonne");
        db2.Plants.FindPlant("broken").ShouldBeNull();
    }

    private Task WritePlantFileAsync(PlantJson plant, string fileName = "tomato")
        => PlantJsonWriter.WriteAsync(_directory, fileName, plant);

    private static PlantJson CreatePlant(
        string nameFr = "Tomate",
        string? descriptionFr = "Plante nécessitant beaucoup d'eau",
        int? version = 1,
        int? frostToleranceC = null,
        SunlightLevel sunlight = SunlightLevel.Low,
        WaterNeedLevel waterNeed = WaterNeedLevel.Low,
        RootDepthLevel rootDepth = RootDepthLevel.Shallow,
        bool nitrogenFixer = false,
        bool dynamicAccumulator = false)
    {
        return new PlantJson
        {
            NameFr = nameFr,
            DescriptionFr = descriptionFr,
            BinomialName = "Solanum lycopersicum",
            Family = PlantFamily.Solanaceae,
            LifeCycle = PlantLifeCycle.Annual,
            FrostToleranceC = frostToleranceC,
            Version = version,
            Needs = new PlantNeedsJson
            {
                Sunlight = sunlight,
                WaterNeed = waterNeed,
                RootDepth = rootDepth
            },
            Traits = new PlantTraitsJson
            {
                NitrogenFixer = nitrogenFixer,
                DynamicAccumulator = dynamicAccumulator
            }
        };
    }
}