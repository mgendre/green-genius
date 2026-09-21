using GreenGenius.Common.Domain.Entities;
using GreenGenius.Common.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Common.Domain;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService) : DbContext(options)
{
    public DbSet<Garden> Gardens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new GardenConfiguration(currentUserService));
    }
}
