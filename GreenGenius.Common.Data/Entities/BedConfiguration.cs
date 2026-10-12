using GreenGenius.Common.Data.Constants;
using GreenGenius.Common.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class BedConfiguration(ApplicationDbContext dbContext) : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.ToTable("beds");

        builder.HasKey(bed => bed.Id);
        builder.Property(bed => bed.Id).ValueGeneratedNever();

        builder.Property(bed => bed.Name)
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);

        builder.Property(bed => bed.GardenId);
        builder.Property(bed => bed.OwnerId);

        builder.HasOne<Garden>()
            .WithMany()
            .HasForeignKey(bed => bed.GardenId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(bed => new { bed.GardenId, bed.OwnerId, bed.Name });

        builder.HasMany(bed => bed.BedPlants)
            .WithOne(bp => bp.Bed)
            .HasForeignKey(bp => bp.BedId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureOwnerIdQueryFilter(dbContext);
    }
}
