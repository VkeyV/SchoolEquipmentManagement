using SchoolEquipmentManagement.Web.ViewModels.Inventory;

namespace SchoolEquipmentManagement.Web.Services.Inventory;

public interface IInventoryViewModelFactory
{
    Task<InventorySessionIndexViewModel> CreateIndexViewModelAsync();
    Task<InventorySessionDetailsViewModel?> CreateDetailsViewModelAsync(int id);
    Task<InventoryCheckViewModel?> CreateCheckViewModelAsync(int sessionId, int equipmentId);
}
