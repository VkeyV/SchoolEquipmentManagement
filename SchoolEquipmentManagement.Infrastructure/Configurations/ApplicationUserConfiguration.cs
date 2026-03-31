using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(x => x.NormalizedUserName)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.Email)
                .HasMaxLength(256);

            builder.Property(x => x.NormalizedEmail)
                .HasMaxLength(256);

            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(x => x.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.TwoFactorEnabled)
                .IsRequired();

            builder.Property(x => x.FailedSignInAttempts)
                .IsRequired();

            builder.Property(x => x.LockoutEndUtc);

            builder.Property(x => x.LastSignInAt);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.HasIndex(x => x.NormalizedUserName)
                .IsUnique();

            builder.HasIndex(x => x.NormalizedEmail)
                .IsUnique()
                .HasFilter("[NormalizedEmail] IS NOT NULL");
        }
    }
}
