using System.Net;
using GreenGenius.Api.Constants;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GreenGenius.App.IntegrationTests;

public class GreenGeniusAppStartupTest(
    IntegrationTestsApplicationFixture integrationFixture)
    : AbstractApiIntegrationTest(integrationFixture)
{
    private readonly IntegrationTestsApplicationFixture _integrationFixture = integrationFixture;

    [Fact]
    public async Task GetHealth_ShouldBeOk()
    {
        var response = await CreateClient().GetAsync(RouteConstants.Health);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task QueryGardens_WhenIntegrationConfigured_ShouldReachContainer()
    {
        _integrationFixture.IsPostgresContainerRunning().ShouldBeTrue();

        await ExecuteInScopeAsync(async ctx =>
        {
            await ctx.Gardens.ToListAsync();
        });
    }
}
