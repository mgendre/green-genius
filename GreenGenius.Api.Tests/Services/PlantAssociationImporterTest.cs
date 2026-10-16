using GreenGenius.App.Services;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Enums;
using GreenGenius.Common.Data.Security;
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
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(service => service.GetCurrentUserId()).Returns(OwnerId);

        return new ApplicationDbContext(options, currentUser.Object);
    }

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
        string fileName,
        string plantA,
        string plantB,
        Polarity polarity = Polarity.Beneficial,
        Mechanism mechanism = Mechanism.PestDeterrent,
        Evidence evidence = Evidence.Established,
        string? noteFr = "Note")
    {
        return PlantJsonWriter.WriteAsync(_directory, fileName, new PlantAssociationJson
        {
            PlantA = plantA,
            PlantB = plantB,
            Polarity = polarity,
            Mechanism = mechanism,
            Evidence = evidence,
            NoteFr = noteFr
        });
    }

    private async Task<int> ImportAsync(ILogger<PlantAssociationImporter>? logger = null)
    {
        await using var db = NewDbContext();
        var importer = new PlantAssociationImporter(db, logger ?? NullLogger<PlantAssociationImporter>.Instance, _directory);
        return await importer.ImportAsync();
    }

    [Fact]
    public async Task ImportAsync_WhenMapping_ShouldMapEveryAssociationField()
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await WriteAssociationAsync("carrot-onion", "carrot", "onion",
            Polarity.Antagonistic, Mechanism.SharedPestOrDisease, Evidence.Limited, "NoteFr");

        var count = await ImportAsync();

        count.ShouldBe(1);
        await using var db2 = NewDbContext();
        var association = await db2.PlantAssociations.SingleAsync();
        association.PlantAId.ShouldBe(carrot.Id);
        association.PlantBId.ShouldBe(onion.Id);
        association.Polarity.ShouldBe(Polarity.Antagonistic);
        association.Mechanism.ShouldBe(Mechanism.SharedPestOrDisease);
        association.Evidence.ShouldBe(Evidence.Limited);
        association.NoteFr.ShouldBe("NoteFr");
    }

    [Fact]
    public async Task ImportAsync_WhenNoteFrChanges_ShouldUpdateTheRowAndAddNone()
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await WriteAssociationAsync("carrot-onion", "carrot", "onion", noteFr: "Ancienne note");
        await ImportAsync();

        await WriteAssociationAsync("carrot-onion", "carrot", "onion", noteFr: "Nouvelle note");
        await ImportAsync();

        await using var db2 = NewDbContext();
        var associations = await db2.PlantAssociations.ToListAsync();
        associations.Count.ShouldBe(1);
        associations[0].PlantAId.ShouldBe(carrot.Id);
        associations[0].PlantBId.ShouldBe(onion.Id);
        associations[0].NoteFr.ShouldBe("Nouvelle note");
    }

    [Fact]
    public async Task ImportAsync_WhenFileGivesTheReversePair_ShouldStoreTheSamePair()
    {
        var carrot = await PersistPlantAsync("carrot");
        var onion = await PersistPlantAsync("onion");
        await WriteAssociationAsync("carrot-onion", "carrot", "onion");
        await ImportAsync();

        await WriteAssociationAsync("onion-carrot", "onion", "carrot", noteFr: "Note inverse");
        await ImportAsync();

        await using var db2 = NewDbContext();
        var associations = await db2.PlantAssociations.ToListAsync();
        associations.Count.ShouldBe(1);
        associations[0].PlantAId.ShouldBe(carrot.Id);
        associations[0].PlantBId.ShouldBe(onion.Id);
        associations[0].NoteFr.ShouldBe("Note inverse");
    }

    [Fact]
    public async Task ImportAsync_WhenFileRemoved_ShouldDeleteTheAssociation()
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await WriteAssociationAsync("carrot-onion", "carrot", "onion");
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
        await WriteAssociationAsync("carrot-ghost", "carrot", "ghost");
        await WriteAssociationAsync("carrot-onion", "carrot", "onion");

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

    [Fact]
    public async Task ImportAsync_WhenJsonInvalid_ShouldSkipAndImportTheOthers()
    {
        await PersistPlantAsync("carrot");
        await PersistPlantAsync("onion");
        await File.WriteAllTextAsync(Path.Combine(_directory, "broken.json"), "{ pas du json");
        await WriteAssociationAsync("carrot-onion", "carrot", "onion");

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
        await WriteAssociationAsync("carrot-onion", "carrot", "onion");
        await ImportAsync();

        await using var db = NewDbContext();
        var importer = new PlantAssociationImporter(db, NullLogger<PlantAssociationImporter>.Instance, Path.Combine(_directory, "absent"));
        var count = await importer.ImportAsync();

        count.ShouldBe(0);
        await using var db2 = NewDbContext();
        (await db2.PlantAssociations.CountAsync()).ShouldBe(1);
    }
}
