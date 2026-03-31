using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Application.Interfaces.Repositories
{
    public interface IInventorySessionRepository
    {
        Task<List<InventorySession>> GetAllAsync();
        Task<InventorySession?> GetByIdAsync(int id);
        Task AddAsync(InventorySession session);
        Task UpdateAsync(InventorySession session);
        Task SaveChangesAsync();
    }
}
