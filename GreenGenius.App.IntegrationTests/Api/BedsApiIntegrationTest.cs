using System.Net;
using System.Net.Http.Json;
using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Beds.Dtos;
using GreenGenius.Api.Features.Beds.Handlers;
using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GreenGenius.App.IntegrationTests.Api;

public class BedsApiIntegrationTest(
    IntegrationTestsApplicationFixture integrationFixture)
    : AbstractApiIntegrationTest(integrationFixture)
{
    private static string GetBedsUrl(Guid gardenId)
    {
        return RouteConstants.Gardens + "/" + gardenId + RouteConstants.Beds;
    }

    private static string GetBedUrl(Guid gardenId, Guid bedId)
    {
        return GetBedsUrl(gardenId) + "/" + bedId;
    }

    private static string GetBedAssociationsUrl(Guid gardenId, Guid bedId)
    {
        return GetBedUrl(gardenId, bedId) + "/associations";
    }

    [Fact]
    public async Task CreateBed_ShouldInsert()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);

        var result = await CreateClient().PostAndRead<BedDto, CreateBedDto>(
            GetBedsUrl(garden.Id), new CreateBedDto
            {
                Name = "TestBed"
            });

        result.Response.StatusCode.ShouldBe(HttpStatusCode.Created);
        result.Value!.Id.ShouldNotBe(Guid.Empty);
        result.Value.Name.ShouldBe("TestBed");
        result.Value.GardenId.ShouldBe(garden.Id);
        result.Value.Plants.ShouldNotBe(null);
        result.Value.Plants.ShouldBeEmpty();

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.Beds.AnyAsync(b => b.Name == "TestBed" && b.GardenId == garden.Id)).ShouldBeTrue();
        });
    }

    [Fact]
    public async Task CreateBed_WhenGardenDoesNotExist_ShouldBeNotFound()
    {
        var response = await CreateClient().PostAsJsonAsync(
            GetBedsUrl(Guid.NewGuid()),
            new { name = "TestBed" });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBed_WhenGardenNotOwned_ShouldBeNotFound()
    {
        var garden = await PersistGardenAsync(Guid.NewGuid());

        var response = await CreateClient().PostAsJsonAsync(
            GetBedsUrl(garden.Id),
            new { name = "TestBed" });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListBeds_ShouldReturnBedsOfGardenSortedByName()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        await PersistBedAsync(garden.Id, DefaultCurrentUser, "ZBed");
        await PersistBedAsync(garden.Id, DefaultCurrentUser, "ABed");

        var beds = await CreateClient().GetAndReadList<BedDto>(
            GetBedsUrl(garden.Id));

        beds.Count.ShouldBe(2);
        beds[0].Name.ShouldBe("ABed");
        beds[1].Name.ShouldBe("ZBed");
        beds[0].Plants.ShouldNotBeNull();
        beds[0].Plants.ShouldBeEmpty();
    }

    [Fact]
    public async Task ListBeds_WhenGardenDoesNotExist_ShouldBeNotFound()
    {
        var response = await CreateClient().GetAsync(
            GetBedsUrl(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListBeds_WhenGardenNotOwned_ShouldBeNotFound()
    {
        var garden = await PersistGardenAsync(Guid.NewGuid());

        var response = await CreateClient().GetAsync(
            GetBedsUrl(garden.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListBeds_ShouldIncludePlantsOfBeds()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        await ExecuteInScopeAsync(async ctx => { await ctx.LinkBedPlantAsync(bed.Id); });

        var beds = await CreateClient().GetAndReadList<BedDto>(
            GetBedsUrl(garden.Id));

        beds.Count.ShouldBe(1);
        beds[0].Plants.Count.ShouldBe(1);
        beds[0].Plants[0].NameFr.ShouldBe("Tomate");
    }

    [Fact]
    public async Task UpdateBed_ShouldRenameAndReplacePlants()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        Plant? plante = null;
        Plant? tomate = null;
        await ExecuteInScopeAsync(async ctx =>
        {
            plante = await ctx.PersistPlantAsync();
            tomate = await ctx.PersistPlantAsync();
        });

        var result = await CreateClient().PutAndRead<BedDto, UpdateBedDto>(
            GetBedUrl(garden.Id, bed.Id), new UpdateBedDto
            {
                Name = "NewName",
                PlantIds = [tomate!.Id, plante!.Id]
            });

        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Value!.Name.ShouldBe("NewName");
        result.Value.Id.ShouldBe(bed.Id);
        result.Value.Plants.Count.ShouldBe(2);
    }

    [Fact]
    public async Task UpdateBed_WhenPlantsReplaced_ShouldReturnTheNewPlantsSortedByName()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        Plant alpha = null!;
        Plant bravo = null!;
        Plant charlie = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            alpha = await ctx.PersistPlantAsync("Alpha");
            bravo = await ctx.PersistPlantAsync("Bravo");
            charlie = await ctx.PersistPlantAsync("Charlie");
            await ctx.LinkBedPlantAsync(bed.Id, alpha);
            await ctx.LinkBedPlantAsync(bed.Id, bravo);
        });

        var result = await CreateClient().PutAndRead<BedDto, UpdateBedDto>(
            GetBedUrl(garden.Id, bed.Id), new UpdateBedDto
            {
                Name = "MyBed",
                PlantIds = [bravo.Id, charlie.Id]
            });

        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Value!.Plants.Count.ShouldBe(2);
        result.Value.Plants.Select(p => p.NameFr).ToList().ShouldBeInOrder(SortDirection.Ascending);
        result.Value.Plants.Select(p => p.Id).ToList().ShouldContain(bravo.Id);
        result.Value.Plants.Select(p => p.Id).ToList().ShouldContain(charlie.Id);

        await ExecuteInScopeAsync(async ctx =>
        {
            var linkedPlantIds = await ctx.BedPlants
                .Where(bp => bp.BedId == bed.Id)
                .Select(bp => bp.PlantId)
                .ToListAsync();

            linkedPlantIds.Count.ShouldBe(2);
            linkedPlantIds.ShouldContain(bravo.Id);
            linkedPlantIds.ShouldContain(charlie.Id);
            linkedPlantIds.ShouldNotContain(alpha.Id);
        });
    }

    [Fact]
    public async Task UpdateBed_WhenPlantIdsEmpty_ShouldRemoveEveryPlant()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        await ExecuteInScopeAsync(async ctx =>
        {
            await ctx.LinkBedPlantAsync(bed.Id);
            await ctx.LinkBedPlantAsync(bed.Id);
        });

        var result = await CreateClient().PutAndRead<BedDto, UpdateBedDto>(
            GetBedUrl(garden.Id, bed.Id), new UpdateBedDto
            {
                Name = "MyBed",
                PlantIds = []
            });

        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Value!.Plants.ShouldBeEmpty();

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.BedPlants.AnyAsync(bp => bp.BedId == bed.Id)).ShouldBeFalse();
        });
    }

    [Fact]
    public async Task UpdateBed_WhenBedUnknown_ShouldBeNotFound()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var update = new UpdateBedDto
        {
            Name = "NewName",
            PlantIds = []
        };

        var response = await CreateClient().PutAsJsonAsync(
            GetBedUrl(garden.Id, Guid.NewGuid()), update);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBed_WhenBedNotOwned_ShouldBeNotFound()
    {
        var otherOwner = Guid.NewGuid();
        var otherGarden = await PersistGardenAsync(otherOwner);
        var bed = await PersistBedAsync(otherGarden.Id, otherOwner, "OldName");

        var response = await CreateClient().PutAsJsonAsync(
            GetBedUrl(otherGarden.Id, bed.Id),
            new { name = "NewName", plantIds = Array.Empty<Guid>() });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await ExecuteInScopeAsync(async ctx =>
        {
            var stored = await ctx.Beds.IgnoreQueryFilters().SingleAsync(b => b.Id == bed.Id);
            stored.Name.ShouldBe("OldName");
        });
    }

    [Fact]
    public async Task UpdateBed_WhenBedInOtherGarden_ShouldBeNotFound()
    {
        var myGarden = await PersistGardenAsync(DefaultCurrentUser);
        var otherGardenOfMine = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(myGarden.Id, DefaultCurrentUser, "OldName");

        var response = await CreateClient().PutAsJsonAsync(
            GetBedUrl(otherGardenOfMine.Id, bed.Id),
            new { name = "NewName", plantIds = Array.Empty<Guid>() });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await ExecuteInScopeAsync(async ctx =>
        {
            var stored = await ctx.Beds.SingleAsync(b => b.Id == bed.Id);
            stored.Name.ShouldBe("OldName");
            stored.GardenId.ShouldBe(myGarden.Id);
        });
    }

    [Fact]
    public async Task UpdateBed_WhenPlantIdNotFound_ShouldBeNotFound()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        var response = await CreateClient().PutAsJsonAsync(
            GetBedUrl(garden.Id, bed.Id),
            new { name = "NewName", plantIds = new[] { Guid.NewGuid() } });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBed_WhenPlantIdsContainDuplicates_ShouldReturnUniquePlants()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        Plant plante = null!;
        await ExecuteInScopeAsync(async ctx => { plante = await ctx.PersistPlantAsync(); });

        var result = await CreateClient().PutAndRead<BedDto, UpdateBedDto>(
            GetBedUrl(garden.Id, bed.Id), new UpdateBedDto
            {
                Name = "MyBed",
                PlantIds = [plante.Id, plante.Id]
            });

        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Value!.Id.ShouldBe(bed.Id);
        result.Value.Plants.Count.ShouldBe(1);
        result.Value.Plants[0].Id.ShouldBe(plante.Id);
    }

    [Fact]
    public async Task DeleteBed_ShouldRemoveBedAndLinks()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        Plant plant = null!;
        await ExecuteInScopeAsync(async ctx => { plant = await ctx.PersistPlantAsync(); });
        await ExecuteInScopeAsync(async ctx => { await ctx.LinkBedPlantAsync(bed.Id, plant); });

        var response = await CreateClient().DeleteAsync(
            GetBedUrl(garden.Id, bed.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.Beds.FindAsync(bed.Id)).ShouldBeNull();
            (await ctx.BedPlants.AnyAsync(bp => bp.BedId == bed.Id)).ShouldBeFalse();
        });

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.Beds.CountAsync(b => b.GardenId == garden.Id)).ShouldBe(0);
        });

        await ExecuteInScopeAsync(async ctx => { (await ctx.Plants.AnyAsync(p => p.Id == plant.Id)).ShouldBeTrue(); });
    }

    [Fact]
    public async Task DeleteBed_WhenUnknown_ShouldBeNotFound()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var response = await CreateClient().DeleteAsync(
            GetBedUrl(garden.Id, Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBed_WhenNotOwned_ShouldBeNotFound()
    {
        var otherOwner = Guid.NewGuid();
        var otherGarden = await PersistGardenAsync(otherOwner);
        var bed = await PersistBedAsync(otherGarden.Id, otherOwner);

        var response = await CreateClient().DeleteAsync(
            GetBedUrl(otherGarden.Id, bed.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.Beds.IgnoreQueryFilters().AnyAsync(b => b.Id == bed.Id)).ShouldBeTrue();
        });
    }

    [Fact]
    public async Task DeleteBed_WhenBedInOtherGarden_ShouldBeNotFound()
    {
        var myGarden = await PersistGardenAsync(DefaultCurrentUser);
        var otherGardenOfMine = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(myGarden.Id, DefaultCurrentUser);

        var response = await CreateClient().DeleteAsync(
            GetBedUrl(otherGardenOfMine.Id, bed.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await ExecuteInScopeAsync(async ctx => { (await ctx.Beds.AnyAsync(b => b.Id == bed.Id)).ShouldBeTrue(); });
    }

    [Fact]
    public async Task DeleteGarden_ShouldCascadeDeleteBeds()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        await PersistBedAsync(garden.Id, DefaultCurrentUser);

        var response = await CreateClient().DeleteAsync(
            RouteConstants.Gardens + "/" + garden.Id);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.Beds.AnyAsync(b => b.GardenId == garden.Id)).ShouldBeFalse();
        });
    }

    [Fact]
    public async Task ReadBedPlants_WhenBedNotOwned_ShouldNotReturnTheLinks()
    {
        var myGarden = await PersistGardenAsync(DefaultCurrentUser);
        var otherGarden = await PersistGardenAsync(Guid.NewGuid());
        var myBed = await PersistBedAsync(myGarden.Id, DefaultCurrentUser);
        var otherBed = await PersistBedAsync(otherGarden.Id, Guid.NewGuid());

        Plant myPlant = null!;
        Plant otherPlant = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            myPlant = await ctx.PersistPlantAsync();
            otherPlant = await ctx.PersistPlantAsync();
            await ctx.LinkBedPlantAsync(myBed.Id, myPlant);
            await ctx.LinkBedPlantAsync(otherBed.Id, otherPlant);
        });

        Guid[] plantIds = [myPlant.Id, otherPlant.Id];

        await ExecuteInScopeAsync(async ctx =>
        {
            var links = await ctx.BedPlants
                .Where(bp => ((IEnumerable<Guid>)plantIds).Contains(bp.PlantId))
                .ToListAsync();

            links.Count.ShouldBe(1);
            links[0].BedId.ShouldBe(myBed.Id);
        });
    }

    [Fact]
    public async Task ListBedAssociations_ShouldReturnPairsOfTheBed()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        var tomatoId = Guid.Empty;
        var garlicId = Guid.Empty;
        await ExecuteInScopeAsync(async ctx =>
        {
            var tomato = await ctx.PersistPlantAsync();
            var garlic = await ctx.PersistPlantAsync("Ail", family: PlantFamily.Amaryllidaceae);
            tomatoId = tomato.Id;
            garlicId = garlic.Id;
            await ctx.LinkBedPlantAsync(bed.Id, tomato);
            await ctx.LinkBedPlantAsync(bed.Id, garlic);
            await ctx.PersistPlantAssociationAsync(tomato, garlic,
                evidence: Evidence.Limited, noteFr: "note-ail");
        });

        var associations = await CreateClient().GetAndReadList<PlantAssociationDto>(
            GetBedAssociationsUrl(garden.Id, bed.Id));

        associations.Count.ShouldBe(1);
        var association = associations[0];
        association.PlantA.Id.ShouldBe(tomatoId);
        association.PlantA.NameFr.ShouldBe("Tomate");
        association.PlantB.Id.ShouldBe(garlicId);
        association.PlantB.NameFr.ShouldBe("Ail");
        association.PlantB.Family.ShouldBe(PlantFamily.Amaryllidaceae);
        association.Effect.ShouldBe(Effect.Beneficial);
        association.Mechanisms.ShouldBe([Mechanism.PestDeterrent]);
        association.Evidence.ShouldBe(Evidence.Limited);
        association.NoteFr.ShouldBe("note-ail");
    }

    [Fact]
    public async Task ListBedAssociations_WhenAssociationHasAPlantOutsideTheBed_ShouldExcludeIt()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        await ExecuteInScopeAsync(async ctx =>
        {
            var tomato = await ctx.PersistPlantAsync();
            var garlic = await ctx.PersistPlantAsync("Ail");
            var zucchini = await ctx.PersistPlantAsync();
            await ctx.LinkBedPlantAsync(bed.Id, tomato);
            await ctx.LinkBedPlantAsync(bed.Id, garlic);
            await ctx.PersistPlantAssociationAsync(tomato, zucchini);
            await ctx.PersistPlantAssociationAsync(tomato, garlic);
        });

        var associations = await CreateClient().GetAndReadList<PlantAssociationDto>(
            GetBedAssociationsUrl(garden.Id, bed.Id));

        associations.Count.ShouldBe(1);
        associations[0].PlantB.NameFr.ShouldBe("Ail");
    }

    [Fact]
    public async Task ListBedAssociations_WhenMixedEffect_ShouldListAntagonisticFirst()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        await ExecuteInScopeAsync(async ctx =>
        {
            var alpha = await ctx.PersistPlantAsync("Alpha");
            var bravo = await ctx.PersistPlantAsync("Bravo");
            var charlie = await ctx.PersistPlantAsync("Charlie");
            await ctx.LinkBedPlantAsync(bed.Id, alpha);
            await ctx.LinkBedPlantAsync(bed.Id, bravo);
            await ctx.LinkBedPlantAsync(bed.Id, charlie);
            await ctx.PersistPlantAssociationAsync(alpha, bravo);
            await ctx.PersistPlantAssociationAsync(alpha, charlie, effect: Effect.Antagonistic);
        });

        var associations = await CreateClient().GetAndReadList<PlantAssociationDto>(
            GetBedAssociationsUrl(garden.Id, bed.Id));

        associations.Count.ShouldBe(2);
        associations[0].Effect.ShouldBe(Effect.Antagonistic);
        associations[0].PlantB.NameFr.ShouldBe("Charlie");
        associations[1].Effect.ShouldBe(Effect.Beneficial);
        associations[1].PlantB.NameFr.ShouldBe("Bravo");
    }

    [Fact]
    public async Task ListBedAssociations_ShouldListMechanismsSortedByName()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        await ExecuteInScopeAsync(async ctx =>
        {
            var tomato = await ctx.PersistPlantAsync();
            var garlic = await ctx.PersistPlantAsync();
            await ctx.LinkBedPlantAsync(bed.Id, tomato);
            await ctx.LinkBedPlantAsync(bed.Id, garlic);
            await ctx.PersistPlantAssociationAsync(tomato, garlic,
                mechanisms: [Mechanism.NutrientAccumulation, Mechanism.Allelopathy, Mechanism.GroundCover]);
        });

        var associations = await CreateClient().GetAndReadList<PlantAssociationDto>(
            GetBedAssociationsUrl(garden.Id, bed.Id));

        associations.Single().Mechanisms
            .ShouldBe([Mechanism.Allelopathy, Mechanism.GroundCover, Mechanism.NutrientAccumulation]);
    }

    [Fact]
    public async Task ListBedAssociations_WhenBedHoldsNoAssociatedPair_ShouldReturnEmptyList()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, DefaultCurrentUser);

        await ExecuteInScopeAsync(async ctx => { await ctx.LinkBedPlantAsync(bed.Id); });

        var result = await CreateClient().GetAndRead<List<PlantAssociationDto>>(
            GetBedAssociationsUrl(garden.Id, bed.Id));

        result.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Value!.ShouldBeEmpty();
    }

    [Fact]
    public async Task ListBedAssociations_WhenGardenDoesNotExist_ShouldBeNotFound()
    {
        var response = await CreateClient().GetAsync(
            GetBedAssociationsUrl(Guid.NewGuid(), Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListBedAssociations_WhenGardenNotOwned_ShouldBeNotFound()
    {
        var garden = await PersistGardenAsync(Guid.NewGuid());
        var bed = await PersistBedAsync(garden.Id, garden.OwnerId);

        var response = await CreateClient().GetAsync(GetBedAssociationsUrl(garden.Id, bed.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListBedAssociations_WhenBedUnknown_ShouldBeNotFound()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);

        var response = await CreateClient().GetAsync(
            GetBedAssociationsUrl(garden.Id, Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListBedAssociations_WhenBedInOtherGarden_ShouldBeNotFound()
    {
        var myGarden = await PersistGardenAsync(DefaultCurrentUser);
        var otherGardenOfMine = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(myGarden.Id, DefaultCurrentUser);

        var response = await CreateClient().GetAsync(
            GetBedAssociationsUrl(otherGardenOfMine.Id, bed.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListBedAssociations_WhenBedNotOwned_ShouldBeNotFound()
    {
        var otherOwner = Guid.NewGuid();
        var otherGarden = await PersistGardenAsync(otherOwner);
        var bed = await PersistBedAsync(otherGarden.Id, otherOwner);

        var response = await CreateClient().GetAsync(
            GetBedAssociationsUrl(otherGarden.Id, bed.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<Garden> PersistGardenAsync(Guid owner)
    {
        Garden garden = null!;
        await ExecuteInScopeAsync(async ctx => { garden = await ctx.PersistGardenAsync(owner); });
        return garden;
    }

    private async Task<Bed> PersistBedAsync(Guid gardenId, Guid ownerId, string? name = null)
    {
        Bed bed = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            bed = name is null
                ? await ctx.PersistBedAsync(gardenId, ownerId)
                : await ctx.PersistBedAsync(gardenId, ownerId, name);
        });
        return bed;
    }
}