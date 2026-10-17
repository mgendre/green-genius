using GreenGenius.Api.Tests.Infra;
using GreenGenius.App.Services;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace GreenGenius.Api.Tests.Services;

public class PlantAssociationImporterTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly string _directory;

    public PlantAssociationImporterTest()
    {
        _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_directory);
    }

    private ApplicationDbContext NewDbContext()
        => InMemoryDbContextFactory.Create(_databaseName, OwnerId);

    private async Task<Plant> PersistPlantAsync(string key)
    {
        var plant = new Plant
        {
            Id = Guid.NewGuid(),
            Key = key,
            NameFr = key,
            Needs = new PlantNeeds(),
            Traits = new PlantTraits()
        };
        await using var db = NewDbContext();
        db.Plants.Add(plant);
        await db.SaveChangesAsync();
        return plant;
    }

    private Task WriteAssociationAsync(
        string plantA = "carrot",
        string plantB = "onion",
        Effect effect = Effect.Beneficial,
        Mechanism[]? mechanisms = null,
        Evidence evidence = Evidence.Established,
        string? noteFr = "Note",
        string fileName = "carrot-onion")
    {
        return PlantJsonWriter.WriteAsync(_directory, fileName, new PlantAssociationJson
        {
            PlantA = plantA,
            PlantB = plantB,
            Effect = effect,
            Mechanisms = (mechanisms ?? [Mechanism.PestDeterrent]).ToList(),
            Evidence = evidence,
            NoteFr = noteFr
        });
    }

    private async Task<int> ImportAsync(ILogger<PlantAssociationImporter>? logger = null)
    {
        await using var db = NewDbContext();
        var importer =
            new PlantAssociationImporter(db, logger ?? NullLogger<PlantAssociationImporter>.Instance, _directory);
        return await importer.ImportAsync();
    }

    [Fact]
    public async Task ImportAsync_WhenMapping_ShouldMapEveryAssociationField()
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await WriteAssociationAsync("carrot", "onion",
            Effect.Antagonistic, [Mechanism.SharedPestOrDisease, Mechanism.Allelopathy], Evidence.Limited, "NoteFr");

        var count = await ImportAsync();

        count.ShouldBe(1);
        await using var db2 = NewDbContext();
        var association = await db2.PlantAssociations
            .Include(plantAssociation => plantAssociation.Mechanisms)
            .SingleAsync();
        association.PlantAId.ShouldBe(carrot.Id);
        association.PlantBId.ShouldBe(onion.Id);
        association.Effect.ShouldBe(Effect.Antagonistic);
        association.Mechanisms.Select(mechanism => mechanism.Mechanism).OrderBy(mechanism => mechanism.ToString())
            .ToList()
            .ShouldBe([Mechanism.Allelopathy, Mechanism.SharedPestOrDisease]);
        association.Evidence.ShouldBe(Evidence.Limited);
        association.NoteFr.ShouldBe("NoteFr");
    }

    [Fact]
    public async Task ImportAsync_WhenNoteFrChanges_ShouldUpdateTheRowAndAddNone()
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await WriteAssociationAsync("carrot", "onion", noteFr: "Ancienne note");
        await ImportAsync();

        await WriteAssociationAsync("carrot", "onion", noteFr: "Nouvelle note");
        await ImportAsync();

        await using var db2 = NewDbContext();
        var associations = await db2.PlantAssociations.ToListAsync();
        associations.Count.ShouldBe(1);
        associations[0].PlantAId.ShouldBe(carrot.Id);
        associations[0].PlantBId.ShouldBe(onion.Id);
        associations[0].NoteFr.ShouldBe("Nouvelle note");
    }

    [Fact]
    public async Task ImportAsync_WhenMechanismsChange_ShouldReplaceTheListAndAddNoAssociation()
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await WriteAssociationAsync(mechanisms: [Mechanism.PestDeterrent, Mechanism.ShadeOrShelter]);
        await ImportAsync();

        await WriteAssociationAsync(mechanisms: [Mechanism.GroundCover, Mechanism.Allelopathy]);
        await ImportAsync();

        await using var db2 = NewDbContext();
        (await db2.PlantAssociations.CountAsync()).ShouldBe(1);
        var association = await db2.PlantAssociations
            .Include(plantAssociation => plantAssociation.Mechanisms)
            .SingleAsync();
        association.Mechanisms.Select(mechanism => mechanism.Mechanism.ToString()).OrderBy(mechanism => mechanism)
            .ToList()
            .ShouldBe(["Allelopathy", "GroundCover"]);
    }

    [Fact]
    public async Task ImportAsync_WhenMechanismWrittenTwice_ShouldStoreOneRowAndImportTheOthers()
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await PersistPlantAsync("potato");
        await PersistPlantAsync("tomato");
        await WriteAssociationAsync(mechanisms: [Mechanism.PestDeterrent, Mechanism.PestDeterrent], noteFr: "Duplique");
        await WriteAssociationAsync("potato", "tomato", fileName: "potato-tomato");

        var count = await ImportAsync();

        count.ShouldBe(2);
        await using var db2 = NewDbContext();
        (await db2.PlantAssociations.CountAsync()).ShouldBe(2);
        var association = await db2.PlantAssociations
            .Include(plantAssociation => plantAssociation.Mechanisms)
            .SingleAsync(plantAssociation => plantAssociation.NoteFr == "Duplique");
        association.Mechanisms.Count.ShouldBe(1);
        association.Mechanisms.Single().Mechanism.ShouldBe(Mechanism.PestDeterrent);
    }

    [Fact]
    public async Task ImportAsync_WhenMechanismWrittenTwiceOnAnExistingRow_ShouldKeepOneRow()
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await WriteAssociationAsync(mechanisms: [Mechanism.GroundCover]);
        await ImportAsync();

        await WriteAssociationAsync(mechanisms: [Mechanism.PestDeterrent, Mechanism.PestDeterrent]);
        await ImportAsync();

        await using var db2 = NewDbContext();
        var association = await db2.PlantAssociations
            .Include(plantAssociation => plantAssociation.Mechanisms)
            .SingleAsync();
        association.Mechanisms.Count.ShouldBe(1);
        association.Mechanisms.Single().Mechanism.ShouldBe(Mechanism.PestDeterrent);
    }

    [Fact]
    public async Task ImportAsync_WhenFileUnchanged_ShouldKeepEveryRow()
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await WriteAssociationAsync(mechanisms: [Mechanism.GroundCover, Mechanism.PestDeterrent]);

        var first = await ImportAsync();
        var second = await ImportAsync();

        second.ShouldBe(first);
        await using var db2 = NewDbContext();
        var association = await db2.PlantAssociations
            .Include(plantAssociation => plantAssociation.Mechanisms)
            .SingleAsync();
        association.NoteFr.ShouldBe("Note");
        association.Effect.ShouldBe(Effect.Beneficial);
        association.Mechanisms.Select(mechanism => mechanism.Mechanism.ToString()).OrderBy(mechanism => mechanism)
            .ToList()
            .ShouldBe(["GroundCover", "PestDeterrent"]);
    }

    [Theory]
    [InlineData("""{"PlantA":"carrot","PlantB":"onion","Effect":0,"Mechanisms":[],"Evidence":0}""")]
    [InlineData("""{"PlantA":"carrot","PlantB":"onion","Effect":0,"Evidence":0}""")]
    public async Task ImportAsync_WhenFileHasNoMechanism_ShouldSkipWithWarningAndImportTheOthers(
        string fileWithoutMechanism)
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await File.WriteAllTextAsync(Path.Combine(_directory, "carrot-empty.json"), fileWithoutMechanism);
        await WriteAssociationAsync();

        var logger = new Mock<ILogger<PlantAssociationImporter>>();
        var count = await ImportAsync(logger.Object);

        count.ShouldBe(1);
        await using var db2 = NewDbContext();
        var association = await db2.PlantAssociations
            .Include(plantAssociation => plantAssociation.Mechanisms)
            .SingleAsync();
        association.Mechanisms.Count.ShouldBe(1);
        logger.Verify(entry => entry.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("carrot-empty")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WhenFileGivesTheReversePair_ShouldStoreTheSamePairAndKeepTheFileNamedFirst()
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await WriteAssociationAsync();
        await ImportAsync();

        await WriteAssociationAsync("onion", "carrot", noteFr: "Note inverse", fileName: "onion-carrot");
        var logger = new Mock<ILogger<PlantAssociationImporter>>();
        await ImportAsync(logger.Object);

        await using var db2 = NewDbContext();
        var associations = await db2.PlantAssociations.ToListAsync();
        associations.Count.ShouldBe(1);
        associations[0].PlantAId.ShouldBe(carrot.Id);
        associations[0].PlantBId.ShouldBe(onion.Id);
        associations[0].NoteFr.ShouldBe("Note");
        logger.Verify(entry => entry.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("onion-carrot")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WhenFileRemoved_ShouldDeleteTheAssociation()
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await WriteAssociationAsync();
        await ImportAsync();

        File.Delete(Path.Combine(_directory, "carrot-onion.json"));
        await ImportAsync();

        await using var db2 = NewDbContext();
        (await db2.PlantAssociations.AnyAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task ImportAsync_WhenPlantKeyUnknown_ShouldSkipWithWarningAndImportTheOthers()
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await WriteAssociationAsync(plantB: "ghost", fileName: "carrot-ghost");
        await WriteAssociationAsync("carrot", "onion");

        var logger = new Mock<ILogger<PlantAssociationImporter>>();
        var count = await ImportAsync(logger.Object);

        count.ShouldBe(1);
        await using var db2 = NewDbContext();
        var associations = await db2.PlantAssociations.ToListAsync();
        associations.Count.ShouldBe(1);
        associations[0].PlantAId.ShouldBe(carrot.Id);
        associations[0].PlantBId.ShouldBe(onion.Id);
        logger.Verify(entry => entry.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("carrot-ghost")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Theory]
    [InlineData("""{"PlantA":null,"PlantB":"onion","Effect":0,"Mechanisms":["PestDeterrent"],"Evidence":0}""")]
    [InlineData("""{"PlantA":"carrot","PlantB":null,"Effect":0,"Mechanisms":["PestDeterrent"],"Evidence":0}""")]
    [InlineData("""{"PlantA":"carrot","PlantB":"onion","Effect":0,"Mechanisms":null,"Evidence":0}""")]
    public async Task ImportAsync_WhenFileHasANullField_ShouldSkipWithWarningAndImportTheOthers(
        string fileWithNullField)
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await File.WriteAllTextAsync(Path.Combine(_directory, "carrot-null.json"), fileWithNullField);
        await WriteAssociationAsync("carrot", "onion");

        var logger = new Mock<ILogger<PlantAssociationImporter>>();
        var count = await ImportAsync(logger.Object);

        count.ShouldBe(1);
        await using var db2 = NewDbContext();
        var associations = await db2.PlantAssociations
            .Include(plantAssociation => plantAssociation.Mechanisms)
            .ToListAsync();
        associations.Count.ShouldBe(1);
        associations[0].PlantAId.ShouldBe(carrot.Id);
        associations[0].PlantBId.ShouldBe(onion.Id);
        associations[0].Mechanisms.Single().Mechanism.ShouldBe(Mechanism.PestDeterrent);
        logger.Verify(entry => entry.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("carrot-null")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WhenPlantEqualsItself_ShouldSkipWithWarningAndImportTheOthers()
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await WriteAssociationAsync(plantB: "carrot", fileName: "carrot-carrot");
        await WriteAssociationAsync("carrot", "onion");

        var logger = new Mock<ILogger<PlantAssociationImporter>>();
        var count = await ImportAsync(logger.Object);

        count.ShouldBe(1);
        await using var db2 = NewDbContext();
        var associations = await db2.PlantAssociations.ToListAsync();
        associations.Count.ShouldBe(1);
        associations[0].PlantAId.ShouldBe(carrot.Id);
        associations[0].PlantBId.ShouldBe(onion.Id);
        (await db2.PlantAssociations.AnyAsync(plantAssociation =>
                plantAssociation.PlantAId == plantAssociation.PlantBId))
            .ShouldBeFalse();
        logger.Verify(entry => entry.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("carrot-carrot")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Theory]
    [InlineData("carrot", "onion")]
    [InlineData("onion", "carrot")]
    public async Task ImportAsync_WhenTwoFilesNameTheSamePair_ShouldImportTheFileNamedFirstAndSkipTheOther(
        string plantA, string plantB)
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await WriteAssociationAsync(plantB, plantA, noteFr: "Nomme en second", fileName: "b-later");
        await WriteAssociationAsync(plantA, plantB, noteFr: "Nomme en premier", fileName: "a-earlier");

        var logger = new Mock<ILogger<PlantAssociationImporter>>();
        var count = await ImportAsync(logger.Object);

        count.ShouldBe(1);
        await using var db2 = NewDbContext();
        var association = await db2.PlantAssociations.SingleAsync();
        association.PlantAId.ShouldBe(carrot.Id);
        association.PlantBId.ShouldBe(onion.Id);
        association.NoteFr.ShouldBe("Nomme en premier");
        logger.Verify(entry => entry.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("b-later")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WhenJsonInvalid_ShouldSkipAndImportTheOthers()
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await File.WriteAllTextAsync(Path.Combine(_directory, "broken.json"), "{ pas du json");
        await WriteAssociationAsync();

        var count = await ImportAsync();

        count.ShouldBe(1);
        await using var db2 = NewDbContext();
        (await db2.PlantAssociations.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task ImportAsync_WhenDirectoryMissing_ShouldImportNothingAndKeepTheRows()
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await WriteAssociationAsync();
        await ImportAsync();

        await using var db = NewDbContext();
        var importer = new PlantAssociationImporter(db, NullLogger<PlantAssociationImporter>.Instance,
            Path.Combine(_directory, "absent"));
        var count = await importer.ImportAsync();

        count.ShouldBe(0);
        await using var db2 = NewDbContext();
        (await db2.PlantAssociations.CountAsync()).ShouldBe(1);
    }
}