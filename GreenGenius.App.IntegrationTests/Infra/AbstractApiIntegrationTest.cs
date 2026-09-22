using GreenGenius.App.IntegrationTests.Extensions;
using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.App.IntegrationTests.Mocks;
using GreenGenius.Common.Domain;
using GreenGenius.Common.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreenGenius.App.IntegrationTests.Infra;

public abstract class AbstractApiIntegrationTest(
        IntegrationTestsApplicationFixture integrationFixture)
    : WebApplicationFactory<Program>,
    IClassFixture<IntegrationTestsApplicationFixture>, IAsyncLifetime
{
    protected Guid DefaultCurrentUser = Guid.NewGuid();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplacePgSql(services);
            UseMocks(services);
        });
    }

    private void UseMocks(IServiceCollection services)
    {
        var currentUserMock = new CurrentUserMock();
        currentUserMock.SetCurrentUser(DefaultCurrentUser);
        services.Replace(typeof(ICurrentUserService), currentUserMock);
    }


    private void ReplacePgSql(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => 
            d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

        if (descriptor is not null)
        {
            services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(integrationFixture.GetPostgresContainerConnectionString()));
        }
        else
        {
            throw new InvalidOperationException("Could not replace DbContext because it was not found");
        }
    }

    protected ApplicationDbContext CreateDbContext()
    {
        return Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
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

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}