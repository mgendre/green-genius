using GreenGenius.Common.Data.Constants;
using GreenGenius.Common.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class PlantNeedsConfiguration : IEntityTypeConfiguration<PlantNeeds>
{
    public void Configure(EntityTypeBuilder<PlantNeeds> builder)
    {
        builder.ToTable("plant_needs");

        builder.HasKey(needs => needs.PlantId);

        builder.Property(needs => needs.Sunlight)
            .HasConversion<string>()
            .HasMaxLength(EntitiesConstants.ShortTextMaxLength);

        builder.Property(needs => needs.WaterNeed)
            .HasConversion<string>()
            .HasMaxLength(EntitiesConstants.ShortTextMaxLength);

        builder.Property(needs => needs.RootDepth)
            .HasConversion<string>()
            .HasMaxLength(EntitiesConstants.ShortTextMaxLength);

        builder.Property(needs => needs.SoilPhMin)
            .HasPrecision(3, 1);

        builder.Property(needs => needs.SoilPhMax)
            .HasPrecision(3, 1);
    }
}
