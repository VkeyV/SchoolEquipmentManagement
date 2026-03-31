using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class InventoryRecordConfiguration : IEntityTypeConfiguration<InventoryRecord>
    {
        public void Configure(EntityTypeBuilder<InventoryRecord> builder)
        {
            builder.ToTable("InventoryRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsFound)
                .IsRequired();

            builder.Property(x => x.ConditionComment)
                .HasMaxLength(1000);

            builder.Property(x => x.CheckedAt)
                .IsRequired();

            builder.Property(x => x.CheckedBy)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasOne(x => x.InventorySession)
                .WithMany(x => x.Records)
                .HasForeignKey(x => x.InventorySessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Equipment)
                .WithMany(x => x.InventoryRecords)
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ActualLocation)
                .WithMany()
                .HasForeignKey(x => x.ActualLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.InventorySessionId, x.EquipmentId })
                .IsUnique();
        }
    }
}
