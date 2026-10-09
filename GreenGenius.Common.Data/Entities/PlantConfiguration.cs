using GreenGenius.Common.Data.Constants;
using GreenGenius.Common.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class PlantConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.ToTable("plants");

        builder.HasKey(plant => plant.Id);
        builder.Property(plant => plant.Id).ValueGeneratedNever();

        builder.Property(plant => plant.NameFr)
            .IsRequired()
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);

        builder.Property(plant => plant.Key)
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);
        builder.HasIndex(plant => plant.Key)
            .IsUnique();

        builder.Property(plant => plant.DescriptionFr)
            .HasMaxLength(EntitiesConstants.LongTextMaxLength);

        builder.Property(plant => plant.BinomialName)
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);

        builder.Property(plant => plant.Family)
            .HasConversion<string>()
            .HasMaxLength(EntitiesConstants.ShortTextMaxLength);

        builder.Property(plant => plant.LifeCycle)
            .HasConversion<string>()
            .HasMaxLength(EntitiesConstants.ShortTextMaxLength);

        builder.HasOne(plant => plant.Needs)
            .WithOne(needs => needs.Plant)
            .HasForeignKey<PlantNeeds>(needs => needs.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(plant => plant.Needs)
            .IsRequired();

        builder.HasOne(plant => plant.Traits)
            .WithOne(traits => traits.Plant)
            .HasForeignKey<PlantTraits>(traits => traits.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(plant => plant.Traits)
            .IsRequired();
    }
}
