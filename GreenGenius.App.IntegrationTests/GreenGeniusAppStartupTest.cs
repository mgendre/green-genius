using Microsoft.AspNetCore.Mvc.Testing;

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
    }
}
