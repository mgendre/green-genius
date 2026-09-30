using GreenGenius.Common.Data.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreenGenius.Common.Data.Extensions;

public static class DataExtensions
{
    public static void ConfigurePersistence(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")
            )
        );
    }

    public static async Task MigrateAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public static void ConfigureOwnerIdQueryFilter<T>(
        this EntityTypeBuilder<T> builder, 
        ApplicationDbContext dbContext) where T : class, IHasOwner
    {
        builder.HasQueryFilter(entity => entity.OwnerId == dbContext.CurrentUserId);
    }
}
