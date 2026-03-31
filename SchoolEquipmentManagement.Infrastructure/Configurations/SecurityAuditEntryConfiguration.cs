using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class SecurityAuditEntryConfiguration : IEntityTypeConfiguration<SecurityAuditEntry>
    {
        public void Configure(EntityTypeBuilder<SecurityAuditEntry> builder)
        {
            builder.ToTable("SecurityAuditEntries");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(64);

            builder.Property(x => x.IsSuccessful)
                .IsRequired();

            builder.Property(x => x.Summary)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.UserName)
                .HasMaxLength(64);

            builder.Property(x => x.TargetUserName)
                .HasMaxLength(64);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(64);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(512);

            builder.Property(x => x.OccurredAt)
                .IsRequired();

            builder.HasIndex(x => x.OccurredAt);
            builder.HasIndex(x => x.EventType);
            builder.HasIndex(x => x.IsSuccessful);
        }
    }
}
