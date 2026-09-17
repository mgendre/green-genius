using GreenGenius.Common.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Domain.Entities;

public sealed class GardenConfiguration : IEntityTypeConfiguration<Garden>
{
    public void Configure(EntityTypeBuilder<Garden> builder)
    {
        builder.ToTable("gardens");
        
        builder.HasKey(garden => garden.Id);
        builder.Property(garden => garden.Id).ValueGeneratedNever();

        builder.Property(garden => garden.Name)
            .IsRequired()
            .HasMaxLength(EntitiesConstants.FieldMaxLength250);
        
        builder.Property(garden => garden.OwnerId)
            .IsRequired();

        builder.HasIndex(garden => new { garden.OwnerId, garden.Name });
    }
}