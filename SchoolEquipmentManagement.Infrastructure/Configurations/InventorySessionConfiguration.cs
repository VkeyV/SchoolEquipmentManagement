using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class InventorySessionConfiguration : IEntityTypeConfiguration<InventorySession>
    {
        public void Configure(EntityTypeBuilder<InventorySession> builder)
        {
            builder.ToTable("InventorySessions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.CreatedBy)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.StartDate)
                .IsRequired();

            builder.Property(x => x.EndDate);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.HasMany(x => x.Records)
                .WithOne(x => x.InventorySession)
                .HasForeignKey(x => x.InventorySessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
