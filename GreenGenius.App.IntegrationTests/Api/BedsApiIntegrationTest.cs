using System.Net;
using System.Net.Http.Json;
using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Beds.Dtos;
using GreenGenius.Api.Features.Beds.Handlers;
using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using GreenGenius.Common.Data.Entities;
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
        await PersistBedAsync(garden.Id, "ZBed", DefaultCurrentUser);
        await PersistBedAsync(garden.Id, "ABed", DefaultCurrentUser);

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
        var bed = await PersistBedAsync(garden.Id, "MyBed", DefaultCurrentUser);

        await ExecuteInScopeAsync(async ctx =>
        {
            await ctx.LinkBedPlantAsync(bed.Id, await ctx.PersistPlantAsync("Tomate"));
        });

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
        var bed = await PersistBedAsync(garden.Id, "OldName", DefaultCurrentUser);

        Plant? plante = null;
        Plant? tomate = null;
        await ExecuteInScopeAsync(async ctx =>
        {
            plante = await ctx.PersistPlantAsync("PlanteA");
            tomate = await ctx.PersistPlantAsync("Tomate");
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
        var bed = await PersistBedAsync(garden.Id, "MyBed", DefaultCurrentUser);

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
        var bed = await PersistBedAsync(garden.Id, "MyBed", DefaultCurrentUser);

        Plant alpha;
        Plant bravo;
        await ExecuteInScopeAsync(async ctx =>
        {
            alpha = await ctx.PersistPlantAsync("Alpha");
            bravo = await ctx.PersistPlantAsync("Bravo");
            await ctx.LinkBedPlantAsync(bed.Id, alpha);
            await ctx.LinkBedPlantAsync(bed.Id, bravo);
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
        var bed = await PersistBedAsync(otherGarden.Id, "OldName", otherOwner);

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
        var bed = await PersistBedAsync(myGarden.Id, "OldName", DefaultCurrentUser);

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
        var bed = await PersistBedAsync(garden.Id, "OldName", DefaultCurrentUser);

        var response = await CreateClient().PutAsJsonAsync(
            GetBedUrl(garden.Id, bed.Id),
            new { name = "NewName", plantIds = new[] { Guid.NewGuid() } });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBed_WhenPlantIdsContainDuplicates_ShouldReturnUniquePlants()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        var bed = await PersistBedAsync(garden.Id, "MyBed", DefaultCurrentUser);

        Plant plante = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            plante = await ctx.PersistPlantAsync("PlanteA");
        });

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
        var bed = await PersistBedAsync(garden.Id, "ToDelete", DefaultCurrentUser);

        Plant plant = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            plant = await ctx.PersistPlantAsync("Tomate");
        });
        await ExecuteInScopeAsync(async ctx =>
        {
            await ctx.LinkBedPlantAsync(bed.Id, plant);
        });

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

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.Plants.AnyAsync(p => p.Id == plant.Id)).ShouldBeTrue();
        });
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
        var bed = await PersistBedAsync(otherGarden.Id, "ToDelete", otherOwner);

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
        var bed = await PersistBedAsync(myGarden.Id, "ToKeep", DefaultCurrentUser);

        var response = await CreateClient().DeleteAsync(
            GetBedUrl(otherGardenOfMine.Id, bed.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.Beds.AnyAsync(b => b.Id == bed.Id)).ShouldBeTrue();
        });
    }

    [Fact]
    public async Task DeleteGarden_ShouldCascadeDeleteBeds()
    {
        var garden = await PersistGardenAsync(DefaultCurrentUser);
        await PersistBedAsync(garden.Id, "TestBed", DefaultCurrentUser);

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
        var myBed = await PersistBedAsync(myGarden.Id, "MyBed", DefaultCurrentUser);
        var otherBed = await PersistBedAsync(otherGarden.Id, "OtherBed", Guid.NewGuid());

        Plant myPlant = null!;
        Plant otherPlant = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            myPlant = await ctx.PersistPlantAsync("MyPlant");
            otherPlant = await ctx.PersistPlantAsync("OtherPlant");
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

    private async Task<Garden> PersistGardenAsync(Guid owner)
    {
        Garden garden = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            garden = await ctx.PersistGardenAsync("MyGarden", owner);
        });
        return garden;
    }

    private async Task<Bed> PersistBedAsync(Guid gardenId, string name, Guid ownerId)
    {
        Bed bed = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            bed = await ctx.PersistBedAsync(Guid.NewGuid(), gardenId, name, ownerId);
        });
        return bed;
    }
}
