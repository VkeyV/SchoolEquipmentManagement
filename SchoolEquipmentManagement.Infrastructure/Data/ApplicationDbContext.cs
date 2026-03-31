using Microsoft.EntityFrameworkCore;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Equipment> Equipment => Set<Equipment>();
        public DbSet<EquipmentType> EquipmentTypes => Set<EquipmentType>();
        public DbSet<EquipmentStatus> EquipmentStatuses => Set<EquipmentStatus>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<EquipmentHistory> EquipmentHistories => Set<EquipmentHistory>();
        public DbSet<InventorySession> InventorySessions => Set<InventorySession>();
        public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();
        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<UserSecurityCode> UserSecurityCodes => Set<UserSecurityCode>();
        public DbSet<SecurityAuditEntry> SecurityAuditEntries => Set<SecurityAuditEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
