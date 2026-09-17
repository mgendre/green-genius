using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace GreenGenius.App.IntegrationTests;

public class GreenGeniusAppStartupTest(WebApplicationFactory<Program> webApplicationFactory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = webApplicationFactory.CreateClient();

    [Fact]
    public async Task Start_ShouldBeHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
