using GreenGenius.Common.Domain.Security;
using GreenGenius.Common.Domain.Services;
using GreenGenius.Common.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreenGenius.Common.Domain.Extensions;

public static class DomainExtensions
{
    public static void ConfigureDomain(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
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
        ICurrentUserService currentUserService) where T : class, IHasOwner
    {
        builder.HasQueryFilter(garden => garden.OwnerId == currentUserService.GetCurrentUserId());
    }
}
