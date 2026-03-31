using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public interface IEquipmentLookupViewModelService
{
    Task PopulateFormAsync(EquipmentCreateViewModel model);
    Task PopulateIndexAsync(EquipmentIndexViewModel model);
    Task PopulateStatusOptionsAsync(EquipmentChangeStatusViewModel model);
    Task PopulateLocationOptionsAsync(EquipmentChangeLocationViewModel model);
    Task<int> GetWrittenOffStatusIdAsync();
    Task<Dictionary<string, Dictionary<string, string>>> CreateHistoryValueResolverAsync();
}
