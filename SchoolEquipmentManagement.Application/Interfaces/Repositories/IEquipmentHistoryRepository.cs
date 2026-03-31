using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Application.Interfaces.Repositories
{
    public interface IEquipmentHistoryRepository
    {
        Task AddAsync(EquipmentHistory historyEntry);
        Task AddRangeAsync(IEnumerable<EquipmentHistory> historyEntries);
        Task SaveChangesAsync();
    }
}
