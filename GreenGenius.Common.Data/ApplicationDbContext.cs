using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Security;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Common.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService) : DbContext(options)
{
    public DbSet<Garden> Gardens { get; set; } = null!;

    // EF re-evaluates a global query filter per query only when it reads a member of the DbContext instance
    public Guid CurrentUserId => currentUserService.GetCurrentUserId();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new GardenConfiguration(this));
    }
}
