using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class BedPlantConfiguration : IEntityTypeConfiguration<BedPlant>
{
    public void Configure(EntityTypeBuilder<BedPlant> builder)
    {
        builder.ToTable("bed_plants");

        builder.HasKey(bp => new { bp.BedId, bp.PlantId });

        builder.HasOne(bp => bp.Plant)
            .WithMany()
            .HasForeignKey(bp => bp.PlantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
