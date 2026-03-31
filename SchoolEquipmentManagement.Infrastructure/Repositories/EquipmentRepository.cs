using Microsoft.EntityFrameworkCore;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Infrastructure.Data;

namespace SchoolEquipmentManagement.Infrastructure.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EquipmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Equipment>> GetAllAsync()
        {
            return await _context.Equipment
                .AsNoTracking()
                .Include(e => e.EquipmentType)
                .Include(e => e.EquipmentStatus)
                .Include(e => e.Location)
                .OrderBy(e => e.InventoryNumber)
                .ToListAsync();
        }

        public async Task<Equipment?> GetByIdAsync(int id)
        {
            return await _context.Equipment
                .Include(e => e.EquipmentType)
                .Include(e => e.EquipmentStatus)
                .Include(e => e.Location)
                .Include(e => e.HistoryEntries)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Equipment?> GetByInventoryNumberAsync(string inventoryNumber)
        {
            inventoryNumber = inventoryNumber.Trim();

            return await _context.Equipment
                .FirstOrDefaultAsync(e => e.InventoryNumber == inventoryNumber);
        }

        public async Task<List<Equipment>> GetFilteredAsync(
            string? search,
            int? typeId,
            int? statusId,
            int? locationId)
        {
            return await BuildFilteredQuery(search, typeId, statusId, locationId)
                .OrderBy(e => e.InventoryNumber)
                .ToListAsync();
        }

        public async Task<int> CountFilteredAsync(
            string? search,
            int? typeId,
            int? statusId,
            int? locationId)
        {
            return await BuildFilteredQuery(search, typeId, statusId, locationId)
                .CountAsync();
        }

        public async Task<List<Equipment>> GetFilteredPageAsync(
            string? search,
            int? typeId,
            int? statusId,
            int? locationId,
            int skip,
            int take)
        {
            return await BuildFilteredQuery(search, typeId, statusId, locationId)
                .OrderBy(e => e.InventoryNumber)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task AddAsync(Equipment equipment)
        {
            await _context.Equipment.AddAsync(equipment);
        }

        public Task UpdateAsync(Equipment equipment)
        {
            _context.Equipment.Update(equipment);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByInventoryNumberAsync(string inventoryNumber)
        {
            inventoryNumber = inventoryNumber.Trim();

            return await _context.Equipment
                .AnyAsync(e => e.InventoryNumber == inventoryNumber);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        private IQueryable<Equipment> BuildFilteredQuery(
            string? search,
            int? typeId,
            int? statusId,
            int? locationId)
        {
            var query = _context.Equipment
                .AsNoTracking()
                .Include(e => e.EquipmentType)
                .Include(e => e.EquipmentStatus)
                .Include(e => e.Location)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(e =>
                    EF.Functions.Like(e.Name, $"%{search}%") ||
                    EF.Functions.Like(e.InventoryNumber, $"%{search}%"));
            }

            if (typeId.HasValue)
            {
                query = query.Where(e => e.EquipmentTypeId == typeId);
            }

            if (statusId.HasValue)
            {
                query = query.Where(e => e.EquipmentStatusId == statusId);
            }

            if (locationId.HasValue)
            {
                query = query.Where(e => e.LocationId == locationId);
            }

            return query;
        }
    }
}
