using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreenGenius.Common.Data.Extensions;

public static class DataExtensions
{
    private const string ConnectionStringName = "DefaultConnection";

    public static void ConfigurePersistence(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
            builder.Configuration.GetConnectionString(ConnectionStringName)
            )
        );
    }

    public static async Task MigrateAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }
}
