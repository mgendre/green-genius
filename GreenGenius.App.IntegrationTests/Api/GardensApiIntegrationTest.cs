using System.Net;
using System.Net.Http.Json;
using GreenGenius.Api.Features.Gardens;
using GreenGenius.Api.Features.Gardens.Handlers;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
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
        var response = await CreateClient().PostAsJsonAsync(GardensApi.BasePattern, NewCreateGardenDto(name));
        
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await GetDbContext().Gardens.AnyAsync(g => g.Name == name)).ShouldBeTrue();
    }
    
    [Fact]
    public async Task CreateGarden_ShouldUseCurrentUser()
    {
        await CreateClient().PostAsJsonAsync(GardensApi.BasePattern, NewCreateGardenDto());
        
        (await GetDbContext().Gardens.AnyAsync(g => g.OwnerId == DefaultCurrentUser)).ShouldBeTrue();
    }

    private static CreateGardenHandler.CreateGardenDto NewCreateGardenDto(string name = "Garden")
    {
        return new CreateGardenHandler.CreateGardenDto
        {
            Name = name
        };
    }
}