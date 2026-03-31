using Microsoft.EntityFrameworkCore;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Infrastructure.Data;

namespace SchoolEquipmentManagement.Infrastructure.Repositories
{
    public class InventoryRecordRepository : IInventoryRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryRecord?> GetBySessionAndEquipmentAsync(int sessionId, int equipmentId)
        {
            return await _context.InventoryRecords
                .Include(x => x.Equipment)
                .Include(x => x.ActualLocation)
                .FirstOrDefaultAsync(x => x.InventorySessionId == sessionId && x.EquipmentId == equipmentId);
        }

        public async Task<List<InventoryRecord>> GetLatestByEquipmentIdsAsync(IEnumerable<int> equipmentIds)
        {
            var ids = equipmentIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new List<InventoryRecord>();
            }

            var records = await _context.InventoryRecords
                .AsNoTracking()
                .Include(x => x.InventorySession)
                .Include(x => x.ActualLocation)
                .Where(x => ids.Contains(x.EquipmentId))
                .ToListAsync();

            return records
                .GroupBy(x => x.EquipmentId)
                .Select(group => group
                    .OrderByDescending(x => x.CheckedAt)
                    .ThenByDescending(x => x.Id)
                    .First())
                .ToList();
        }

        public async Task AddAsync(InventoryRecord record)
        {
            await _context.InventoryRecords.AddAsync(record);
        }

        public Task UpdateAsync(InventoryRecord record)
        {
            _context.InventoryRecords.Update(record);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
