using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;
using Testcontainers.PostgreSql;

namespace GreenGenius.App.IntegrationTests.Fixtures;

[UsedImplicitly]
public class IntegrationTestsApplicationFixture : IAsyncLifetime
{
    private const string PostgresContainer = "postgres:18.6";
    
    static IntegrationTestsApplicationFixture()
    {
        if (Environment.GetEnvironmentVariable("DOCKER_HOST") is not null)
        {
            // Docker detected, no more configuration needed
            return;
        }

        // Assume Podman as container engine
        var podmanSock = $"unix://{Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR")}/podman/podman.sock";
        Environment.SetEnvironmentVariable("DOCKER_HOST", podmanSock);
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
    }
    
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder(PostgresContainer)
        .WithUsername("integration")
        .WithPassword(Guid.NewGuid().ToString())
        .WithDatabase("green-genius-integration-tests")
        .WithHostname("127.0.0.1")
        .WithPortBinding(5432, assignRandomHostPort: true)
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        Console.WriteLine("Ok");
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    public bool IsPostgresContainerRunning()
    {
        return _dbContainer.State == TestcontainersStates.Running;
    }

    public string GetPostgresContainerConnectionString()
    {
        return _dbContainer.GetConnectionString();
    }
}