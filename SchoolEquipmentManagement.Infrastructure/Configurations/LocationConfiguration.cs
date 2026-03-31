using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("Locations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Building)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Room)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasIndex(x => new { x.Building, x.Room })
                .IsUnique();

            builder.HasMany(x => x.EquipmentItems)
                .WithOne(x => x.Location)
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
