using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class PlantTraitsConfiguration : IEntityTypeConfiguration<PlantTraits>
{
    public void Configure(EntityTypeBuilder<PlantTraits> builder)
    {
        builder.ToTable("plant_traits");

        builder.HasKey(traits => traits.PlantId);
    }
}