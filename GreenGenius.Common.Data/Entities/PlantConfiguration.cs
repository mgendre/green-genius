using GreenGenius.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class PlantConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.ToTable("plants");

        builder.Property(plant => plant.Id).ValueGeneratedNever();

        builder.Property(plant => plant.NameFr)
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);

        builder.Property(plant => plant.Key)
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);
        builder.HasIndex(plant => plant.Key)
            .IsUnique();

        builder.Property(plant => plant.DescriptionFr)
            .HasMaxLength(EntitiesConstants.LongTextMaxLength);

        builder.Property(plant => plant.BinomialName)
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);

        builder.Property(plant => plant.Emoji)
            .HasMaxLength(4);

        builder.HasOne(plant => plant.Needs)
            .WithOne()
            .HasForeignKey<PlantNeeds>(needs => needs.PlantId);

        builder.HasOne(plant => plant.Traits)
            .WithOne()
            .HasForeignKey<PlantTraits>(traits => traits.PlantId);
    }
}