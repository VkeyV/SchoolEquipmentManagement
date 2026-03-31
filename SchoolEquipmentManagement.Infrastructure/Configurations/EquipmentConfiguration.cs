using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.ToTable("Equipment");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.InventoryNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.SerialNumber)
                .HasMaxLength(100);

            builder.Property(x => x.Model)
                .HasMaxLength(150);

            builder.Property(x => x.Manufacturer)
                .HasMaxLength(150);

            builder.Property(x => x.ResponsiblePerson)
                .HasMaxLength(150);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.HasIndex(x => x.InventoryNumber)
                .IsUnique();

            builder.HasOne(x => x.EquipmentType)
                .WithMany(x => x.EquipmentItems)
                .HasForeignKey(x => x.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EquipmentStatus)
                .WithMany(x => x.EquipmentItems)
                .HasForeignKey(x => x.EquipmentStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Location)
                .WithMany(x => x.EquipmentItems)
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.HistoryEntries)
                .WithOne(x => x.Equipment)
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.InventoryRecords)
                .WithOne(x => x.Equipment)
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
