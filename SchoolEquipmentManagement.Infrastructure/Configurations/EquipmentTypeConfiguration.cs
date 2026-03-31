using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class EquipmentTypeConfiguration : IEntityTypeConfiguration<EquipmentType>
    {
        public void Configure(EntityTypeBuilder<EquipmentType> builder)
        {
            builder.ToTable("EquipmentTypes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.HasMany(x => x.EquipmentItems)
                .WithOne(x => x.EquipmentType)
                .HasForeignKey(x => x.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
