
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Application.Interfaces.Repositories
{
    public interface IDictionaryRepository
    {
        Task<List<EquipmentType>> GetEquipmentTypesAsync();
        Task<List<EquipmentStatus>> GetEquipmentStatusesAsync();
        Task<List<Location>> GetLocationsAsync();
    }
}
