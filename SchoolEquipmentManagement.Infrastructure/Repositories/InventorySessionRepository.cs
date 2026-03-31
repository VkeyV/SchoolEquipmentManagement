using Microsoft.EntityFrameworkCore;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Infrastructure.Data;

namespace SchoolEquipmentManagement.Infrastructure.Repositories
{
    public class InventorySessionRepository : IInventorySessionRepository
    {
        private readonly ApplicationDbContext _context;

        public InventorySessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventorySession>> GetAllAsync()
        {
            return await _context.InventorySessions
                .Include(x => x.Records)
                .ThenInclude(x => x.Equipment)
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<InventorySession?> GetByIdAsync(int id)
        {
            return await _context.InventorySessions
                .Include(x => x.Records)
                .ThenInclude(x => x.Equipment)
                .ThenInclude(x => x.Location)
                .Include(x => x.Records)
                .ThenInclude(x => x.ActualLocation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(InventorySession session)
        {
            await _context.InventorySessions.AddAsync(session);
        }

        public Task UpdateAsync(InventorySession session)
        {
            _context.InventorySessions.Update(session);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
