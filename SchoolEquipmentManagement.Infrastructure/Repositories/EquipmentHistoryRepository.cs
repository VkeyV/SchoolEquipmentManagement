using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Infrastructure.Data;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;

namespace SchoolEquipmentManagement.Infrastructure.Repositories
{
    public class EquipmentHistoryRepository : IEquipmentHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public EquipmentHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EquipmentHistory historyEntry)
        {
            await _context.EquipmentHistories.AddAsync(historyEntry);
        }

        public async Task AddRangeAsync(IEnumerable<EquipmentHistory> historyEntries)
        {
            await _context.EquipmentHistories.AddRangeAsync(historyEntries);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
