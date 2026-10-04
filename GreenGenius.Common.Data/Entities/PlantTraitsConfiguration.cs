using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class PlantTraitsConfiguration : IEntityTypeConfiguration<PlantTraits>
{
    public void Configure(EntityTypeBuilder<PlantTraits> builder)
    {
        builder.ToTable("plant_traits");

        builder.HasKey(traits => traits.PlantId);

        builder.Property(traits => traits.NitrogenFixer);

        builder.Property(traits => traits.DynamicAccumulator);

        builder.Property(traits => traits.PollinatorFriendly);

        builder.Property(traits => traits.DroughtTolerant);
    }
}
