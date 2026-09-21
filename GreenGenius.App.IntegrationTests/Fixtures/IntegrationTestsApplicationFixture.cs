using DotNet.Testcontainers.Containers;
using GreenGenius.App.IntegrationTests.Mocks;
using GreenGenius.Common.Domain;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace GreenGenius.App.IntegrationTests.Fixtures;

[UsedImplicitly]
public class IntegrationTestsApplicationFixture : IAsyncLifetime
{
    private const string PostgresContainer = "postgres:18.6";

    private Respawner? _respawner;
    private NpgsqlConnection? _dbConnection;
    
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
        if (_dbConnection is null)
        {
            await _dbContainer.StartAsync();
            
            var connectionString = _dbContainer.GetConnectionString();
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connectionString);
            _dbConnection = new NpgsqlConnection(connectionString);
            await _dbConnection.OpenAsync();
            
            var dbContext = new ApplicationDbContext(builder.Options, new CurrentUserMock());
            await dbContext.Database.MigrateAsync();
            
            await CreateRespawnerAsync();
        }
    }
    
    private async Task CreateRespawnerAsync()
    {
        _respawner = await Respawner.CreateAsync(_dbConnection!, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Table("public", "__EFMigrationsHistory")]
        });
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        
        if (_dbConnection is not null)
        {
            await _dbConnection.DisposeAsync();
        }
    }

    public bool IsPostgresContainerRunning()
    {
        return _dbContainer.State == TestcontainersStates.Running;
    }

    public string GetPostgresContainerConnectionString()
    {
        return _dbContainer.GetConnectionString();
    }

    public async Task ResetDbAsync()
    {
        if (_respawner is not null && _dbConnection is not null)
        {
            await _respawner.ResetAsync(_dbConnection);
        }
    }
}