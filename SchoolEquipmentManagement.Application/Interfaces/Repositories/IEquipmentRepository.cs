using SchoolEquipmentManagement.Domain.Entities;


namespace SchoolEquipmentManagement.Application.Interfaces.Repositories
{
    public interface IEquipmentRepository
    {
        Task<List<Equipment>> GetAllAsync();

        Task<Equipment?> GetByIdAsync(int id);

        Task<Equipment?> GetByInventoryNumberAsync(string inventoryNumber);

        Task<List<Equipment>> GetFilteredAsync(
            string? search,
            int? typeId,
            int? statusId,
            int? locationId);

        Task<int> CountFilteredAsync(
            string? search,
            int? typeId,
            int? statusId,
            int? locationId);

        Task<List<Equipment>> GetFilteredPageAsync(
            string? search,
            int? typeId,
            int? statusId,
            int? locationId,
            int skip,
            int take);

        Task AddAsync(Equipment equipment);

        Task UpdateAsync(Equipment equipment);

        Task<bool> ExistsByInventoryNumberAsync(string inventoryNumber);

        Task SaveChangesAsync();
    }
}
