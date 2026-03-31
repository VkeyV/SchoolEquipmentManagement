using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Configurations
{
    public class UserSecurityCodeConfiguration : IEntityTypeConfiguration<UserSecurityCode>
    {
        public void Configure(EntityTypeBuilder<UserSecurityCode> builder)
        {
            builder.ToTable("UserSecurityCodes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Purpose)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(x => x.ChallengeToken)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(x => x.CodeHash)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.HasIndex(x => x.ChallengeToken)
                .IsUnique();

            builder.HasIndex(x => new { x.UserId, x.Purpose });

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
