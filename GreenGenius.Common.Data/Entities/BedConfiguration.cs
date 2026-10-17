using GreenGenius.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class BedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.ToTable("beds");

        builder.Property(bed => bed.Id).ValueGeneratedNever();

        builder.Property(bed => bed.Name)
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);

        builder.HasOne<Garden>()
            .WithMany()
            .HasForeignKey(bed => bed.GardenId)
            .IsRequired();

        builder.HasIndex(bed => new { bed.GardenId, bed.OwnerId, bed.Name });

        builder.HasMany(bed => bed.BedPlants)
            .WithOne(bp => bp.Bed)
            .HasForeignKey(bp => bp.BedId);
    }
}