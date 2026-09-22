using GreenGenius.Common.Domain.Entities;
using GreenGenius.Common.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Common.Domain;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService) : DbContext(options)
{
    public DbSet<Garden> Gardens { get; set; } = null!;

    // required for re-evaluation
    public Guid CurrentUserId => currentUserService.GetCurrentUserId();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new GardenConfiguration(this));
    }
}
