using GreenGenius.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class PlantAssociationConfiguration : IEntityTypeConfiguration<PlantAssociation>
{
    public void Configure(EntityTypeBuilder<PlantAssociation> builder)
    {
        builder.ToTable("plant_associations");

        builder.Property(association => association.Id).ValueGeneratedNever();

        builder.Property(association => association.NoteFr)
            .HasMaxLength(EntitiesConstants.LongTextMaxLength);

        builder.HasIndex(association => new { association.PlantAId, association.PlantBId })
            .IsUnique();

        builder.HasOne(association => association.PlantA)
            .WithMany()
            .HasForeignKey(association => association.PlantAId);

        builder.HasOne(association => association.PlantB)
            .WithMany()
            .HasForeignKey(association => association.PlantBId);
    }
}