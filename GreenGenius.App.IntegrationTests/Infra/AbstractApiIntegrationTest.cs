using GreenGenius.App.IntegrationTests.Fixtures;
using GreenGenius.Common.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreenGenius.App.IntegrationTests.Infra;

public abstract class AbstractApiIntegrationTest(
        IntegrationTestsApplicationFixture integrationFixture)
    : WebApplicationFactory<Program>,
    IClassFixture<IntegrationTestsApplicationFixture>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

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
        });
    }

    protected ApplicationDbContext GetDbContext()
    {
        return Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}