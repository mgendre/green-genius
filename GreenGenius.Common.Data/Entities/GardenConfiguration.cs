using GreenGenius.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class GardenConfiguration : IEntityTypeConfiguration<Garden>
{
    public void Configure(EntityTypeBuilder<Garden> builder)
    {
        builder.ToTable("gardens");

        builder.Property(garden => garden.Id).ValueGeneratedNever();

        builder.Property(garden => garden.Name)
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);

        builder.HasIndex(garden => new { garden.OwnerId, garden.Name });
    }
}