using System.Net;
using System.Net.Http.Json;
using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Api.Features.Gardens.Handlers;
using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using GreenGenius.Common.Domain.Entities;
using GreenGenius.Infra.Database.Extensions;
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
        var result = CreateClient().PostAndRead<GardenDto, CreateGardenDto>(
            RouteConstants.Gardens, NewCreateGardenDto(name), out var response);
        
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        result!.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe(name);
        result.OwnerId.ShouldBe(DefaultCurrentUser);
        
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

        var gardens = CreateClient().GetAndRead<IList<GardenDto>>(RouteConstants.Gardens, out _);
        
        gardens!.Count.ShouldBe(3);
        gardens.Select(g => g.Name).ToList().ShouldBeInOrder(SortDirection.Ascending);
    }
    
    [Fact]
    public async Task ListGardens_ShouldOnlySortMyGardens()
    {
        Garden myGarden = null!;
        Garden anotherGarden = null!;
        
        await ExecuteInScopeAsync(async ctx =>
        {
            myGarden = await ctx.PersistGardenAsync("MyGarden", DefaultCurrentUser);
            anotherGarden = await ctx.PersistGardenAsync("AnotherGarden", Guid.NewGuid());
        });

        var gardens = CreateClient().GetAndRead<IList<GardenDto>>(RouteConstants.Gardens, out _);

        gardens!.Count.ShouldBe(1);
        gardens.First().Id.ShouldBe(myGarden.Id);
        gardens.Any(g => g.Id == anotherGarden.Id).ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateGarden_ShouldPatchExistingGardenInDb()
    {
        Garden existing = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            existing = await ctx.PersistGardenAsync("MyGarden", DefaultCurrentUser);
        });

        var update = new UpdateGardenDto { Name = "newName" };
        
        var response = CreateClient().PutAndRead<GardenDto, UpdateGardenDto>(
            RouteConstants.Gardens + "/" + existing.Id, update, out _);
        
        response!.Name.ShouldBe("newName");
        
        await ExecuteInScopeAsync(async ctx =>
        {
            var updated = await ctx.Gardens.GetAsync(existing.Id);
            updated.Name.ShouldBe("newName");
        });
    }
    
    [Fact]
    public async Task UpdateGarden_WhenUpdatingUnOwnedGarden_ShouldBeNotFound()
    {
        Garden wrongOwner = null!;
        await ExecuteInScopeAsync(async ctx =>
        {
            wrongOwner = await ctx.PersistGardenAsync("MyGarden", Guid.NewGuid());
        });

        var update = new UpdateGardenDto { Name = "newName" };
        
        var response = await CreateClient().PutAsJsonAsync(RouteConstants.Gardens + "/" + wrongOwner.Id, update);
        
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task UpdateGarden_WhenUpdatingUnknownGarden_ShouldBeNotFound()
    {
        var update = new UpdateGardenDto { Name = "newName" };
        
        var response = await CreateClient().PutAsJsonAsync(RouteConstants.Gardens + "/" + Guid.NewGuid(), update);
        
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static CreateGardenDto NewCreateGardenDto(string name = "Garden")
    {
        return new CreateGardenDto
        {
            Name = name
        };
    }
}