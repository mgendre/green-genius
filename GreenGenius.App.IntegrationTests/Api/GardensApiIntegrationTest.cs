using System.Net;
using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Api.Features.Gardens.Handlers;
using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using GreenGenius.Common.Domain.Entities;
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
            var list = ctx.Gardens.ToList();
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
            await ctx.PersistGardenAsync("AnotherGarden", Guid.NewGuid());
        });

        var gardens = CreateClient().GetAndRead<IList<GardenDto>>(RouteConstants.Gardens, out _);

        gardens!.Count.ShouldBe(1);
        gardens.First().Id.ShouldBe(myGarden.Id);
        gardens.Any(g => g.Id == anotherGarden.Id).ShouldBeFalse();
    }

    private static CreateGardenDto NewCreateGardenDto(string name = "Garden")
    {
        return new CreateGardenDto
        {
            Name = name
        };
    }
}