using SchoolEquipmentManagement.Application.DTOs;

namespace SchoolEquipmentManagement.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<List<InventorySessionListItemDto>> GetSessionsAsync();
        Task<InventorySessionDetailsDto?> GetSessionDetailsAsync(int id);
        Task<int> CreateSessionAsync(CreateInventorySessionDto dto);
        Task StartSessionAsync(int id);
        Task CompleteSessionAsync(int id);
        Task<InventorySessionEquipmentItemDto?> GetCheckItemAsync(int sessionId, int equipmentId);
        Task RecordCheckAsync(InventoryCheckDto dto);
    }
}
