using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Web.ViewModels.Inventory;

namespace SchoolEquipmentManagement.Web.Services.Inventory;

public sealed class InventoryLookupViewModelService : IInventoryLookupViewModelService
{
    private readonly IDictionaryService _dictionaryService;

    public InventoryLookupViewModelService(IDictionaryService dictionaryService)
    {
        _dictionaryService = dictionaryService;
    }

    public async Task PopulateLocationsAsync(InventoryCheckViewModel model)
    {
        var locations = await _dictionaryService.GetLocationsAsync();
        model.Locations = locations
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.ActualLocationId))
            .ToList();
    }
}
