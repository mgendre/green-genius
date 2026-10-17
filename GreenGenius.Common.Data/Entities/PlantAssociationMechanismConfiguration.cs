using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class PlantAssociationMechanismConfiguration : IEntityTypeConfiguration<PlantAssociationMechanism>
{
    public void Configure(EntityTypeBuilder<PlantAssociationMechanism> builder)
    {
        builder.ToTable("plant_association_mechanisms");

        builder.HasKey(mechanism => new { mechanism.PlantAssociationId, mechanism.Mechanism });

        builder.HasOne(mechanism => mechanism.PlantAssociation)
            .WithMany(association => association.Mechanisms)
            .HasForeignKey(mechanism => mechanism.PlantAssociationId);
    }
}