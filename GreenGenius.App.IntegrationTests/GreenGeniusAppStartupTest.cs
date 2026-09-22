using System.Net;
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
    public async Task Application_ShouldBeHealthy()
    {
        var response = await CreateClient().GetAsync("/health");

        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task IntegrationTests_ShouldBeConfigured() // Helps troubleshoot integration tests configuration 
    {
        _integrationFixture.IsPostgresContainerRunning().ShouldBeTrue();
        await CreateDbContext().Gardens.ToListAsync();
    }
}