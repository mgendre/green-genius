using GreenGenius.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Common.Domain;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public required DbSet<Garden> Gardens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new GardenConfiguration());
    }
}
