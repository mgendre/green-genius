using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class BedPlantConfiguration : IEntityTypeConfiguration<BedPlant>
{
    public void Configure(EntityTypeBuilder<BedPlant> builder)
    {
        builder.ToTable("bed_plants");
        builder.HasKey(bedPlant => new { bedPlant.BedId, bedPlant.PlantId });
        builder
            .HasOne(bedPlant => bedPlant.Plant)
            .WithMany()
            .HasForeignKey(bedPlant => bedPlant.PlantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}