using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public sealed class EquipmentLookupViewModelService : IEquipmentLookupViewModelService
{
    private readonly IDictionaryService _dictionaryService;

    public EquipmentLookupViewModelService(IDictionaryService dictionaryService)
    {
        _dictionaryService = dictionaryService;
    }

    public async Task PopulateFormAsync(EquipmentCreateViewModel model)
    {
        var types = await _dictionaryService.GetEquipmentTypesAsync();
        var statuses = await _dictionaryService.GetEquipmentStatusesAsync();
        var locations = await _dictionaryService.GetLocationsAsync();

        model.EquipmentTypes = types
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.EquipmentTypeId))
            .ToList();

        model.EquipmentStatuses = statuses
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.EquipmentStatusId))
            .ToList();

        model.Locations = locations
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.LocationId))
            .ToList();
    }

    public async Task PopulateIndexAsync(EquipmentIndexViewModel model)
    {
        var types = await _dictionaryService.GetEquipmentTypesAsync();
        var statuses = await _dictionaryService.GetEquipmentStatusesAsync();
        var locations = await _dictionaryService.GetLocationsAsync();

        model.EquipmentTypes = types
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.TypeId))
            .ToList();

        model.EquipmentStatuses = statuses
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.StatusId))
            .ToList();

        model.Locations = locations
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.LocationId))
            .ToList();
    }

    public async Task PopulateStatusOptionsAsync(EquipmentChangeStatusViewModel model)
    {
        var statuses = await _dictionaryService.GetEquipmentStatusesAsync();
        model.AvailableStatuses = statuses
            .Where(x => !string.Equals(x.Name, "Списано", StringComparison.OrdinalIgnoreCase))
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.NewStatusId))
            .ToList();
    }

    public async Task PopulateLocationOptionsAsync(EquipmentChangeLocationViewModel model)
    {
        var locations = await _dictionaryService.GetLocationsAsync();
        model.AvailableLocations = locations
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.NewLocationId))
            .ToList();
    }

    public async Task<int> GetWrittenOffStatusIdAsync()
    {
        var writtenOffStatus = (await _dictionaryService.GetEquipmentStatusesAsync())
            .FirstOrDefault(x => string.Equals(x.Name, "Списано", StringComparison.OrdinalIgnoreCase));

        if (writtenOffStatus is null)
        {
            throw new InvalidOperationException("В справочнике отсутствует статус «Списано».");
        }

        return writtenOffStatus.Id;
    }

    public async Task<Dictionary<string, Dictionary<string, string>>> CreateHistoryValueResolverAsync()
    {
        var types = await _dictionaryService.GetEquipmentTypesAsync();
        var statuses = await _dictionaryService.GetEquipmentStatusesAsync();
        var locations = await _dictionaryService.GetLocationsAsync();

        return new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
        {
            ["EquipmentTypeId"] = types.ToDictionary(x => x.Id.ToString(), x => x.Name),
            ["EquipmentStatusId"] = statuses.ToDictionary(x => x.Id.ToString(), x => x.Name),
            ["LocationId"] = locations.ToDictionary(x => x.Id.ToString(), x => x.Name)
        };
    }
}
