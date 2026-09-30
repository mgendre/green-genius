using GreenGenius.Common.Data.Constants;
using GreenGenius.Common.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Entities;

public sealed class GardenConfiguration(ApplicationDbContext dbContext) : IEntityTypeConfiguration<Garden>
{
    public void Configure(EntityTypeBuilder<Garden> builder)
    {
        builder.ToTable("gardens");

        builder.HasKey(garden => garden.Id);
        builder.Property(garden => garden.Id).ValueGeneratedNever();

        builder.Property(garden => garden.Name)
            .IsRequired()
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);

        builder.Property(garden => garden.OwnerId)
            .IsRequired();

        builder.HasIndex(garden => new { garden.OwnerId, garden.Name });

        builder.ConfigureOwnerIdQueryFilter(dbContext);
    }
}