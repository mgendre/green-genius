using System.Net;
using System.Net.Http.Json;
using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Api.Features.Gardens.Handlers;
using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GreenGenius.App.IntegrationTests.Api;

public class GardensApiIntegrationTest(
    IntegrationTestsApplicationFixture integrationFixture)
    : AbstractApiIntegrationTest(integrationFixture)
{
    [Fact]
    public async Task CreateGarden_ShouldInsert()
    {
        const string name = "AnotherGarden";

        var result = await CreateClient().PostAndRead<GardenDto, CreateGardenDto>(
            RouteConstants.Gardens, NewCreateGardenDto(name));

        result.Response.StatusCode.ShouldBe(HttpStatusCode.Created);
        result.Value!.Id.ShouldNotBe(Guid.Empty);
        result.Value.Name.ShouldBe(name);

        await ExecuteInScopeAsync(async ctx =>
        {
            (await ctx.Gardens.AnyAsync(g => g.Name == name)).ShouldBeTrue();
        });
    }

    [Fact]
    public async Task ListGardens_ShouldSortByName()
    {
        await ExecuteInScopeAsync(async ctx =>
        {
            await ctx.PersistGardenAsync("ZName", DefaultCurrentUser);
            await ctx.PersistGardenAsync("AName", DefaultCurrentUser);
            await ctx.PersistGardenAsync("BName", DefaultCurrentUser);
        });

        var gardens = await CreateClient().GetAndReadList<GardenDto>(RouteConstants.Gardens);

        gardens.Count.ShouldBe(3);
        gardens.Select(g => g.Name).ToList().ShouldBeInOrder(SortDirection.Ascending);
    }

    [Fact]
    public async Task ListGardens_WhenGardensOfOtherOwners_ShouldOnlyListMyGardens()
    {
        var myGarden = await PersistGardenAsync(DefaultCurrentUser);
        var anotherGarden = await PersistGardenAsync(Guid.NewGuid());

        var gardens = await CreateClient().GetAndReadList<GardenDto>(RouteConstants.Gardens);

        gardens.Count.ShouldBe(1);
        gardens.First().Id.ShouldBe(myGarden.Id);
        gardens.Any(g => g.Id == anotherGarden.Id).ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateGarden_ShouldUpdateExistingGardenInDb()
    {
        var existing = await PersistGardenAsync(DefaultCurrentUser);
        var update = new UpdateGardenDto { Name = "newName" };

        var result = await CreateClient().PutAndRead<GardenDto, UpdateGardenDto>(GetGardenUrl(existing.Id), update);

        result.Value!.Name.ShouldBe("newName");

        await ExecuteInScopeAsync(async ctx =>
        {
            var updated = await ctx.Gardens.GetAsync(existing.Id);
            updated.Name.ShouldBe("newName");
        });
    }

    [Fact]
    public async Task UpdateGarden_WhenNotOwnedGarden_ShouldBeNotFound()
    {
        var wrongOwner = await PersistGardenAsync(Guid.NewGuid());
        var update = new UpdateGardenDto { Name = "newName" };

        var response = await CreateClient().PutAsJsonAsync(GetGardenUrl(wrongOwner.Id), update);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateGarden_WhenUnknownGarden_ShouldBeNotFound()
    {
        var update = new UpdateGardenDto { Name = "newName" };

        var response = await CreateClient().PutAsJsonAsync(GetGardenUrl(Guid.NewGuid()), update);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGarden_ShouldRemoveInDb()
    {
        var existing = await PersistGardenAsync(DefaultCurrentUser);

        var response = await CreateClient().DeleteAsync(GetGardenUrl(existing.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var gardens = await CreateClient().GetAndReadList<GardenDto>(RouteConstants.Gardens);
        gardens.Select(g => g.Id).ShouldNotContain(existing.Id);
    }

    [Fact]
    public async Task DeleteGarden_WhenNotOwnedGarden_ShouldBeNotFound()
    {
        var wrongOwner = await PersistGardenAsync(Guid.NewGuid());

        var response = await CreateClient().DeleteAsync(GetGardenUrl(wrongOwner.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGarden_WhenUnknownGarden_ShouldBeNotFound()
    {
        var response = await CreateClient().DeleteAsync(GetGardenUrl(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static string GetGardenUrl(Guid id)
    {
        return RouteConstants.Gardens + "/" + id;
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

    private static CreateGardenDto NewCreateGardenDto(string name = "Garden")
    {
        return new CreateGardenDto
        {
            Name = name
        };
    }
}
