using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class EquipmentHistoryConfiguration : IEntityTypeConfiguration<EquipmentHistory>
    {
        public void Configure(EntityTypeBuilder<EquipmentHistory> builder)
        {
            builder.ToTable("EquipmentHistories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ActionType)
                .IsRequired();

            builder.Property(x => x.ChangedField)
                .HasMaxLength(100);

            builder.Property(x => x.OldValue)
                .HasMaxLength(500);

            builder.Property(x => x.NewValue)
                .HasMaxLength(500);

            builder.Property(x => x.Comment)
                .HasMaxLength(1000);

            builder.Property(x => x.ChangedBy)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.ChangedAt)
                .IsRequired();

            builder.HasOne(x => x.Equipment)
                .WithMany(x => x.HistoryEntries)
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
