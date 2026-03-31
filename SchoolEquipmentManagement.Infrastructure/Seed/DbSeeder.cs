using Microsoft.EntityFrameworkCore;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Infrastructure.Data;
using SchoolEquipmentManagement.Infrastructure.Security;

namespace SchoolEquipmentManagement.Infrastructure.Seed
{
    public static class DbSeeder
    {
        private const string LegacyRepairStatusName = "На ремонте";
        private const string RepairStatusName = "В ремонте";

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            await SeedUsers(context);
            await SeedEquipmentTypes(context);
            await SeedEquipmentStatuses(context);
            await SeedLocations(context);
            await SeedEquipment(context);
        }

        private static async Task SeedUsers(ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync())
                return;

            var users = new List<ApplicationUser>
            {
                new("admin", "Администратор", "admin@example.local", PasswordHashUtility.HashPassword("Admin123!"), UserRole.Administrator),
                new("tech", "Техник", "tech@example.local", PasswordHashUtility.HashPassword("Tech123!"), UserRole.Technician),
                new("responsible", "Ответственный", "responsible@example.local", PasswordHashUtility.HashPassword("Responsible123!"), UserRole.Responsible),
                new("viewer", "Наблюдатель", "viewer@example.local", PasswordHashUtility.HashPassword("Viewer123!"), UserRole.Viewer)
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        private static async Task SeedEquipmentTypes(ApplicationDbContext context)
        {
            if (await context.EquipmentTypes.AnyAsync())
                return;

            var types = new List<EquipmentType>
        {
            new("Системный блок"),
            new("Ноутбук"),
            new("Монитор"),
            new("Принтер"),
            new("МФУ"),
            new("Проектор"),
            new("Интерактивная панель"),
            new("Маршрутизатор")
        };

            await context.EquipmentTypes.AddRangeAsync(types);
            await context.SaveChangesAsync();
        }

        private static async Task SeedEquipmentStatuses(ApplicationDbContext context)
        {
            var existingStatuses = await context.EquipmentStatuses.ToListAsync();
            if (existingStatuses.Count > 0)
            {
                var legacyRepairStatus = existingStatuses.FirstOrDefault(x => x.Name == LegacyRepairStatusName);
                var currentRepairStatus = existingStatuses.FirstOrDefault(x => x.Name == RepairStatusName);

                if (legacyRepairStatus is not null && currentRepairStatus is null)
                {
                    legacyRepairStatus.Update(RepairStatusName, legacyRepairStatus.Description);
                    await context.SaveChangesAsync();
                }

                return;
            }

            var statuses = new List<EquipmentStatus>
        {
            new("В эксплуатации"),
            new(RepairStatusName),
            new("На складе"),
            new("В резерве"),
            new("Списано"),
            new("Требует диагностики")
        };

            await context.EquipmentStatuses.AddRangeAsync(statuses);
            await context.SaveChangesAsync();
        }

        private static async Task SeedLocations(ApplicationDbContext context)
        {
            if (await context.Locations.AnyAsync())
                return;

            var locations = new List<Location>
        {
            new("Главный корпус", "Кабинет 101"),
            new("Главный корпус", "Кабинет 203"),
            new("Лабораторный корпус", "Кабинет 305"),
            new("Лабораторный корпус", "Серверная"),
            new("Склад", "IT-склад")
        };

            await context.Locations.AddRangeAsync(locations);
            await context.SaveChangesAsync();
        }

        private static async Task SeedEquipment(ApplicationDbContext context)
        {
            if (await context.Equipment.AnyAsync())
                return;

            var desktopType = await context.EquipmentTypes.SingleAsync(x => x.Name == "Системный блок");
            var laptopType = await context.EquipmentTypes.SingleAsync(x => x.Name == "Ноутбук");
            var monitorType = await context.EquipmentTypes.SingleAsync(x => x.Name == "Монитор");

            var inUseStatus = await context.EquipmentStatuses.SingleAsync(x => x.Name == "В эксплуатации");
            var reserveStatus = await context.EquipmentStatuses.SingleAsync(x => x.Name == "В резерве");

            var room101 = await context.Locations.SingleAsync(x => x.Building == "Главный корпус" && x.Room == "Кабинет 101");
            var room203 = await context.Locations.SingleAsync(x => x.Building == "Главный корпус" && x.Room == "Кабинет 203");
            var warehouse = await context.Locations.SingleAsync(x => x.Building == "Склад" && x.Room == "IT-склад");

            var equipment = new List<Equipment>
        {
            new(
                inventoryNumber: "INV-0001",
                name: "Рабочая станция преподавателя",
                equipmentTypeId: desktopType.Id,
                equipmentStatusId: inUseStatus.Id,
                locationId: room101.Id,
                manufacturer: "Dell",
                model: "OptiPlex 7090",
                responsiblePerson: "Иванов И.И."
            ),
            new(
                inventoryNumber: "INV-0002",
                name: "Ноутбук ученика",
                equipmentTypeId: laptopType.Id,
                equipmentStatusId: inUseStatus.Id,
                locationId: room203.Id,
                manufacturer: "HP",
                model: "ProBook 450",
                responsiblePerson: "Петров П.П."
            ),
            new(
                inventoryNumber: "INV-0003",
                name: "Резервный монитор",
                equipmentTypeId: monitorType.Id,
                equipmentStatusId: reserveStatus.Id,
                locationId: warehouse.Id,
                manufacturer: "Samsung",
                model: "S24R350"
            )
        };

            await context.Equipment.AddRangeAsync(equipment);
            await context.SaveChangesAsync();
        }
    }
}
