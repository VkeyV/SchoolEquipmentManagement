using SchoolEquipmentManagement.Application.DTOs;

namespace SchoolEquipmentManagement.Application.Interfaces
{
    public interface IDictionaryService
    {
        Task<List<LookupItemDto>> GetEquipmentTypesAsync();
        Task<List<LookupItemDto>> GetEquipmentStatusesAsync();
        Task<List<LookupItemDto>> GetLocationsAsync();
    }
}
