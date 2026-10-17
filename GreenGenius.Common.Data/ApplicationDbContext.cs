using GreenGenius.Common.Data.Constants;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Security;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Common.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService) : DbContext(options)
{
    public DbSet<Garden> Gardens { get; set; } = null!;
    public DbSet<Plant> Plants { get; set; } = null!;
    public DbSet<PlantNeeds> PlantNeeds { get; set; } = null!;
    public DbSet<PlantTraits> PlantTraits { get; set; } = null!;
    public DbSet<Bed> Beds { get; set; } = null!;
    public DbSet<BedPlant> BedPlants { get; set; } = null!;
    public DbSet<PlantAssociation> PlantAssociations { get; set; } = null!;

    // EF re-evaluates a global query filter per query only when it reads a member of the DbContext instance
    public Guid CurrentUserId => currentUserService.GetCurrentUserId();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>()
            .HaveConversion<string>()
            .HaveMaxLength(EntitiesConstants.ShortTextMaxLength);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ApplyOwnerQueryFilters(modelBuilder);
    }

    private void ApplyOwnerQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Garden>().HasQueryFilter(garden => garden.OwnerId == CurrentUserId);
        modelBuilder.Entity<Bed>().HasQueryFilter(bed => bed.OwnerId == CurrentUserId);
        modelBuilder.Entity<BedPlant>().HasQueryFilter(bedPlant => bedPlant.Bed.OwnerId == CurrentUserId);
    }
}