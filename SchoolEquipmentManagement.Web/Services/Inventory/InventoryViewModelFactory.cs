using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.ViewModels.Inventory;

namespace SchoolEquipmentManagement.Web.Services.Inventory;

public sealed class InventoryViewModelFactory : IInventoryViewModelFactory
{
    private readonly IInventoryService _inventoryService;
    private readonly IUserAccessService _userAccessService;

    public InventoryViewModelFactory(
        IInventoryService inventoryService,
        IUserAccessService userAccessService)
    {
        _inventoryService = inventoryService;
        _userAccessService = userAccessService;
    }

    public async Task<InventorySessionIndexViewModel> CreateIndexViewModelAsync()
    {
        var sessions = await _inventoryService.GetSessionsAsync();

        return new InventorySessionIndexViewModel
        {
            CanCreateSession = _userAccessService.HasPermission(ModulePermission.CreateInventorySession),
            Sessions = sessions.Select(x => new InventorySessionListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                CreatedBy = x.CreatedBy,
                CheckedCount = x.CheckedCount,
                FoundCount = x.FoundCount,
                MissingCount = x.MissingCount,
                DiscrepancyCount = x.DiscrepancyCount
            }).ToList()
        };
    }

    public async Task<InventorySessionDetailsViewModel?> CreateDetailsViewModelAsync(int id)
    {
        var session = await _inventoryService.GetSessionDetailsAsync(id);
        if (session is null)
        {
            return null;
        }

        return new InventorySessionDetailsViewModel
        {
            Id = session.Id,
            Name = session.Name,
            StartDate = session.StartDate,
            EndDate = session.EndDate,
            Status = session.Status,
            CreatedBy = session.CreatedBy,
            TotalEquipmentCount = session.TotalEquipmentCount,
            CheckedCount = session.CheckedCount,
            FoundCount = session.FoundCount,
            MissingCount = session.MissingCount,
            DiscrepancyCount = session.DiscrepancyCount,
            CanManageSession = _userAccessService.HasPermission(ModulePermission.ManageInventorySession),
            CanCheckInventory = _userAccessService.HasPermission(ModulePermission.CheckInventory),
            EquipmentItems = session.EquipmentItems.Select(x => new InventorySessionEquipmentItemViewModel
            {
                EquipmentId = x.EquipmentId,
                InventoryNumber = x.InventoryNumber,
                Name = x.Name,
                EquipmentTypeName = x.EquipmentTypeName,
                ExpectedLocationName = x.ExpectedLocationName,
                EquipmentStatusName = x.EquipmentStatusName,
                IsChecked = x.IsChecked,
                IsFound = x.IsFound,
                ActualLocationName = x.ActualLocationName,
                ConditionComment = x.ConditionComment,
                CheckedAt = x.CheckedAt,
                CheckedBy = x.CheckedBy,
                HasLocationDiscrepancy = x.HasLocationDiscrepancy
            }).ToList()
        };
    }

    public async Task<InventoryCheckViewModel?> CreateCheckViewModelAsync(int sessionId, int equipmentId)
    {
        var item = await _inventoryService.GetCheckItemAsync(sessionId, equipmentId);
        if (item is null)
        {
            return null;
        }

        return new InventoryCheckViewModel
        {
            SessionId = sessionId,
            EquipmentId = equipmentId,
            InventoryNumber = item.InventoryNumber,
            Name = item.Name,
            ExpectedLocationName = item.ExpectedLocationName,
            EquipmentStatusName = item.EquipmentStatusName,
            IsFound = item.IsFound ?? true,
            ActualLocationId = item.ActualLocationId,
            ConditionComment = item.ConditionComment
        };
    }
}
