using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Mocks;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GreenGenius.App.IntegrationTests.Infra;

[Collection(IntegrationTestsCollection.Name)]
public abstract class AbstractApiIntegrationTest(
        IntegrationTestsApplicationFixture integrationFixture)
    : WebApplicationFactory<Program>,
    IAsyncLifetime
{
    protected readonly Guid DefaultCurrentUser = Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", integrationFixture.GetPostgresContainerConnectionString());
        builder.UseSetting("PlantsImporterEnabled", "false");
        builder.ConfigureServices(UseMocks);
    }

    private void UseMocks(IServiceCollection services)
    {
        var currentUserMock = new CurrentUserMock();
        currentUserMock.SetCurrentUser(DefaultCurrentUser);
        services.Replace(typeof(ICurrentUserService), currentUserMock);
    }

    protected async Task ExecuteInScopeAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(context);
    }

    public Task InitializeAsync()
    {
        return integrationFixture.ResetDbAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
