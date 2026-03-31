using SchoolEquipmentManagement.Web.ViewModels.Inventory;

namespace SchoolEquipmentManagement.Web.Services.Inventory;

public interface IInventoryLookupViewModelService
{
    Task PopulateLocationsAsync(InventoryCheckViewModel model);
}
