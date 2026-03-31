using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class EquipmentStatusConfiguration : IEntityTypeConfiguration<EquipmentStatus>
    {
        public void Configure(EntityTypeBuilder<EquipmentStatus> builder)
        {
            builder.ToTable("EquipmentStatuses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.HasMany(x => x.EquipmentItems)
                .WithOne(x => x.EquipmentStatus)
                .HasForeignKey(x => x.EquipmentStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
