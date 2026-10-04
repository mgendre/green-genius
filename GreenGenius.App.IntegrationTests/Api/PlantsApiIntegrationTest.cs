using System.Net;
using System.Net.Http.Json;
using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using GreenGenius.Common.Data.Entities;
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
            await ctx.PersistPlantAsync("ZTomate");
            await ctx.PersistPlantAsync("ACourgette");
        });

        var plants = await CreateClient().GetAndReadList<PlantSummaryDto>(RouteConstants.Plants);

        plants.Count.ShouldBe(2);
        plants.Select(p => p.NameFr).ToList().ShouldBeInOrder(SortDirection.Ascending);
        plants.First().Family.ShouldNotBeNull();
        plants.First().Id.ShouldNotBe(Guid.Empty);
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
    }

    [Fact]
    public async Task GetPlant_WhenUnknownPlant_ShouldBeNotFound()
    {
        var response = await CreateClient().GetAsync(GetPlantUrl(Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static string GetPlantUrl(Guid id)
    {
        return RouteConstants.Plants + "/" + id;
    }
}
