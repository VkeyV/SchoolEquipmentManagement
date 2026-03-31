using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Application.Interfaces.Repositories
{
    public interface IInventoryRecordRepository
    {
        Task<InventoryRecord?> GetBySessionAndEquipmentAsync(int sessionId, int equipmentId);
        Task<List<InventoryRecord>> GetLatestByEquipmentIdsAsync(IEnumerable<int> equipmentIds);
        Task AddAsync(InventoryRecord record);
        Task UpdateAsync(InventoryRecord record);
        Task SaveChangesAsync();
    }
}
