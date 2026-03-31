using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public sealed class EquipmentDetailsViewModelFactory : IEquipmentDetailsViewModelFactory
{
    private const int HistoryPageSize = 8;

    private readonly IEquipmentService _equipmentService;
    private readonly IEquipmentLookupViewModelService _lookupService;
    private readonly IUserAccessService _userAccessService;
    private readonly IEquipmentMediaService _equipmentMediaService;

    public EquipmentDetailsViewModelFactory(
        IEquipmentService equipmentService,
        IEquipmentLookupViewModelService lookupService,
        IUserAccessService userAccessService,
        IEquipmentMediaService equipmentMediaService)
    {
        _equipmentService = equipmentService;
        _lookupService = lookupService;
        _userAccessService = userAccessService;
        _equipmentMediaService = equipmentMediaService;
    }

    public async Task<EquipmentDetailsViewModel?> CreateAsync(int id, int historyPage, bool includeFullHistory, Func<int, string> detailsUrlFactory)
    {
        var item = await _equipmentService.GetEquipmentDetailsAsync(id);
        if (item is null)
        {
            return null;
        }

        var historyPageSize = includeFullHistory
            ? Math.Max(item.History.Count, 1)
            : HistoryPageSize;

        var historyTotalCount = item.History.Count;
        var historyTotalPages = historyTotalCount == 0
            ? 1
            : (int)Math.Ceiling((double)historyTotalCount / historyPageSize);

        if (historyPage < 1)
        {
            historyPage = 1;
        }

        if (historyPage > historyTotalPages)
        {
            historyPage = historyTotalPages;
        }

        var fieldValueResolver = await _lookupService.CreateHistoryValueResolverAsync();
        var resolvedHistory = item.History
            .Select(historyItem => new EquipmentHistoryViewModel
            {
                Id = historyItem.Id,
                ActionType = GetHistoryActionDisplayName(historyItem.ActionType),
                ChangedField = GetHistoryFieldDisplayName(historyItem.ChangedField),
                OldValue = ResolveHistoryValue(historyItem.ChangedField, historyItem.OldValue, fieldValueResolver),
                NewValue = ResolveHistoryValue(historyItem.ChangedField, historyItem.NewValue, fieldValueResolver),
                Comment = historyItem.Comment,
                ChangedBy = historyItem.ChangedBy,
                ChangedAt = historyItem.ChangedAt,
                BadgeClass = GetHistoryBadgeClass(historyItem.ActionType)
            })
            .ToList();

        var latestHistoryEntry = item.History.FirstOrDefault();
        var latestInventoryEntry = item.History.FirstOrDefault(x => x.ActionType == "InventoryChecked");
        var (warrantyStatus, warrantySummary, warrantyBadgeClass) = BuildWarrantyPresentation(item.WarrantyEndDate);
        var (lifecycleStage, lifecycleSummary, lifecycleBadgeClass) = BuildLifecyclePresentation(
            item.EquipmentStatusName,
            item.PurchaseDate,
            item.CommissioningDate);
        var warrantyRisk = EquipmentWarrantyPresentation.GetRiskLabel(item.WarrantyEndDate);
        var warrantyRiskBadgeClass = EquipmentWarrantyPresentation.GetRiskBadgeClass(warrantyRisk);

        return new EquipmentDetailsViewModel
        {
            Id = item.Id,
            LocationId = item.LocationId,
            InventoryNumber = item.InventoryNumber,
            Name = item.Name,
            EquipmentType = item.EquipmentTypeName,
            Status = item.EquipmentStatusName,
            Location = item.LocationName,
            SerialNumber = item.SerialNumber,
            Manufacturer = item.Manufacturer,
            Model = item.Model,
            PurchaseDate = item.PurchaseDate,
            CommissioningDate = item.CommissioningDate,
            WarrantyEndDate = item.WarrantyEndDate,
            ResponsiblePerson = item.ResponsiblePerson,
            Notes = item.Notes,
            IsWrittenOff = string.Equals(item.EquipmentStatusName, "Списано", StringComparison.OrdinalIgnoreCase),
            PhotoSource = _equipmentMediaService.ResolvePhotoSource(item.Id, item.Name, item.EquipmentTypeName, item.InventoryNumber),
            QrCodeSource = _equipmentMediaService.BuildQrCodeSource(detailsUrlFactory(item.Id)),
            CodeDataUri = _equipmentMediaService.BuildCodeDataUri(item.InventoryNumber),
            ServiceSummary = BuildServiceSummary(item),
            LifecycleStage = lifecycleStage,
            LifecycleSummary = lifecycleSummary,
            LifecycleBadgeClass = lifecycleBadgeClass,
            WarrantyStatus = warrantyStatus,
            WarrantySummary = warrantySummary,
            WarrantyRisk = warrantyRisk,
            WarrantyRiskBadgeClass = warrantyRiskBadgeClass,
            WarrantyBadgeClass = warrantyBadgeClass,
            LastInventorySummary = BuildLastInventorySummary(latestInventoryEntry),
            OwnershipSummary = BuildOwnershipSummary(item.ResponsiblePerson, item.LocationName),
            HistoryPage = historyPage,
            HistoryPageSize = historyPageSize,
            HistoryTotalCount = historyTotalCount,
            HistoryTotalPages = historyTotalPages,
            LastChangedAt = latestHistoryEntry?.ChangedAt,
            LastChangedBy = latestHistoryEntry?.ChangedBy,
            CanEdit = _userAccessService.HasPermission(ModulePermission.EditEquipment),
            CanChangeStatus = _userAccessService.HasPermission(ModulePermission.ChangeEquipmentStatus),
            CanChangeLocation = _userAccessService.HasPermission(ModulePermission.ChangeEquipmentLocation),
            CanWriteOff = _userAccessService.HasPermission(ModulePermission.WriteOffEquipment),
            Movements = BuildMovementTimeline(item.History, fieldValueResolver),
            History = resolvedHistory
                .Skip((historyPage - 1) * historyPageSize)
                .Take(historyPageSize)
                .ToList()
        };
    }

    private static string BuildOwnershipSummary(string? responsiblePerson, string locationName)
    {
        if (string.IsNullOrWhiteSpace(responsiblePerson))
        {
            return $"Размещено в точке учета: {locationName}. Ответственное лицо не указано.";
        }

        return $"Закреплено за {responsiblePerson}. Текущее размещение: {locationName}.";
    }

    private static string BuildServiceSummary(EquipmentDetailsDto item)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(item.Manufacturer))
        {
            parts.Add(item.Manufacturer.Trim());
        }

        if (!string.IsNullOrWhiteSpace(item.Model))
        {
            parts.Add(item.Model.Trim());
        }

        if (!string.IsNullOrWhiteSpace(item.SerialNumber))
        {
            parts.Add($"серийный номер {item.SerialNumber.Trim()}");
        }

        return parts.Count == 0
            ? "Сервисные реквизиты заполнены частично. При необходимости добавьте производителя, модель и серийный номер."
            : $"{string.Join(", ", parts)}.";
    }

    private static string BuildLastInventorySummary(EquipmentHistoryItemDto? latestInventoryEntry)
    {
        if (latestInventoryEntry is null)
        {
            return "Проверка по инвентаризации пока не зафиксирована.";
        }

        return !string.IsNullOrWhiteSpace(latestInventoryEntry.Comment)
            ? $"{latestInventoryEntry.ChangedAt:dd.MM.yyyy}: {latestInventoryEntry.Comment}"
            : $"Последняя инвентаризация зафиксирована {latestInventoryEntry.ChangedAt:dd.MM.yyyy}.";
    }

    private static (string Stage, string Summary, string BadgeClass) BuildLifecyclePresentation(
        string statusName,
        DateTime? purchaseDate,
        DateTime? commissioningDate)
    {
        var normalizedStatus = statusName.ToLowerInvariant();

        if (normalizedStatus.Contains("спис"))
        {
            return ("Списано", "Оборудование выведено из эксплуатации и хранится только в истории учета.", "bg-danger text-white");
        }

        if (normalizedStatus.Contains("ремонт"))
        {
            return ("Обслуживание", "Объект находится в сервисном цикле и временно недоступен для штатной эксплуатации.", "bg-warning text-dark");
        }

        if (normalizedStatus.Contains("склад") || normalizedStatus.Contains("резерв"))
        {
            return ("Хранение", "Оборудование находится в резерве или на складе и может быть повторно распределено.", "bg-info text-dark");
        }

        if (purchaseDate.HasValue && !commissioningDate.HasValue)
        {
            return ("Подготовка к вводу", "Оборудование закуплено, но дата ввода в эксплуатацию пока не зафиксирована.", "bg-secondary text-white");
        }

        return ("Эксплуатация", "Оборудование используется в рабочем контуре и сопровождается в обычном режиме.", "bg-success text-white");
    }

    private static (string Status, string Summary, string BadgeClass) BuildWarrantyPresentation(DateTime? warrantyEndDate)
    {
        if (!warrantyEndDate.HasValue)
        {
            return ("Гарантия не указана", "Дата окончания гарантии не заполнена в карточке оборудования.", "bg-secondary text-white");
        }

        var daysLeft = (warrantyEndDate.Value.Date - DateTime.Today).Days;

        if (daysLeft < 0)
        {
            return ("Гарантия истекла", $"Срок гарантии завершился {warrantyEndDate.Value:dd.MM.yyyy}. Просрочка: {Math.Abs(daysLeft)} дн.", "bg-danger text-white");
        }

        if (daysLeft <= 30)
        {
            return ("Гарантия скоро истекает", $"До окончания гарантии осталось {daysLeft} дн. Контрольная дата: {warrantyEndDate.Value:dd.MM.yyyy}.", "bg-warning text-dark");
        }

        return ("Гарантия активна", $"Гарантийное покрытие действует до {warrantyEndDate.Value:dd.MM.yyyy}.", "bg-success text-white");
    }

    private static List<EquipmentMovementViewModel> BuildMovementTimeline(
        IEnumerable<EquipmentHistoryItemDto> history,
        Dictionary<string, Dictionary<string, string>> lookupMaps)
    {
        return history
            .Where(h => h.ActionType is "Created" or "StatusChanged" or "LocationChanged" or "InventoryChecked" or "WrittenOff")
            .Select(h => CreateMovementItem(h, lookupMaps))
            .ToList();
    }

    private static EquipmentMovementViewModel CreateMovementItem(
        EquipmentHistoryItemDto historyItem,
        Dictionary<string, Dictionary<string, string>> lookupMaps)
    {
        var oldValue = ResolveHistoryValue(historyItem.ChangedField, historyItem.OldValue, lookupMaps);
        var newValue = ResolveHistoryValue(historyItem.ChangedField, historyItem.NewValue, lookupMaps);

        return historyItem.ActionType switch
        {
            "Created" => new EquipmentMovementViewModel
            {
                OccurredAt = historyItem.ChangedAt,
                EventName = "Постановка на учет",
                Summary = "Оборудование добавлено в реестр.",
                Details = historyItem.Comment,
                ChangedBy = historyItem.ChangedBy,
                BadgeClass = GetHistoryBadgeClass(historyItem.ActionType)
            },
            "StatusChanged" => new EquipmentMovementViewModel
            {
                OccurredAt = historyItem.ChangedAt,
                EventName = "Смена статуса",
                Summary = $"Статус изменен: {oldValue ?? "Не указано"} -> {newValue ?? "Не указано"}.",
                Details = historyItem.Comment,
                ChangedBy = historyItem.ChangedBy,
                BadgeClass = GetHistoryBadgeClass(historyItem.ActionType)
            },
            "LocationChanged" => new EquipmentMovementViewModel
            {
                OccurredAt = historyItem.ChangedAt,
                EventName = "Перемещение",
                Summary = $"Местоположение изменено: {oldValue ?? "Не указано"} -> {newValue ?? "Не указано"}.",
                Details = historyItem.Comment,
                ChangedBy = historyItem.ChangedBy,
                BadgeClass = GetHistoryBadgeClass(historyItem.ActionType)
            },
            "InventoryChecked" => new EquipmentMovementViewModel
            {
                OccurredAt = historyItem.ChangedAt,
                EventName = "Инвентаризация",
                Summary = string.IsNullOrWhiteSpace(historyItem.Comment)
                    ? "Зафиксирован результат инвентаризационной проверки."
                    : historyItem.Comment!,
                Details = string.IsNullOrWhiteSpace(newValue) ? null : newValue,
                ChangedBy = historyItem.ChangedBy,
                BadgeClass = GetHistoryBadgeClass(historyItem.ActionType)
            },
            "WrittenOff" => new EquipmentMovementViewModel
            {
                OccurredAt = historyItem.ChangedAt,
                EventName = "Списание",
                Summary = "Оборудование выведено из эксплуатации.",
                Details = historyItem.Comment,
                ChangedBy = historyItem.ChangedBy,
                BadgeClass = GetHistoryBadgeClass(historyItem.ActionType)
            },
            _ => new EquipmentMovementViewModel
            {
                OccurredAt = historyItem.ChangedAt,
                EventName = GetHistoryActionDisplayName(historyItem.ActionType),
                Summary = historyItem.Comment ?? "Изменение зафиксировано в карточке оборудования.",
                ChangedBy = historyItem.ChangedBy,
                BadgeClass = GetHistoryBadgeClass(historyItem.ActionType)
            }
        };
    }

    private static string? ResolveHistoryValue(
        string? fieldName,
        string? rawValue,
        Dictionary<string, Dictionary<string, string>> lookupMaps)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(fieldName) &&
            lookupMaps.TryGetValue(fieldName, out var map) &&
            map.TryGetValue(rawValue, out var resolvedValue))
        {
            return resolvedValue;
        }

        if (fieldName is "PurchaseDate" or "CommissioningDate" or "WarrantyEndDate" &&
            DateTime.TryParse(rawValue, out var parsedDate))
        {
            return parsedDate.ToString("dd.MM.yyyy");
        }

        return rawValue;
    }

    private static string GetHistoryActionDisplayName(string actionType)
    {
        return actionType switch
        {
            "Created" => "Создание",
            "Updated" => "Редактирование",
            "StatusChanged" => "Смена статуса",
            "LocationChanged" => "Смена местоположения",
            "InventoryChecked" => "Инвентаризация",
            "WrittenOff" => "Списание",
            _ => actionType
        };
    }

    private static string? GetHistoryFieldDisplayName(string? fieldName)
    {
        return fieldName switch
        {
            "InventoryNumber" => "Инвентарный номер",
            "Name" => "Наименование",
            "SerialNumber" => "Серийный номер",
            "Manufacturer" => "Производитель",
            "Model" => "Модель",
            "PurchaseDate" => "Дата покупки",
            "CommissioningDate" => "Дата ввода в эксплуатацию",
            "WarrantyEndDate" => "Дата окончания гарантии",
            "ResponsiblePerson" => "Ответственное лицо",
            "Notes" => "Примечание",
            "InventoryCheck" => "Инвентаризация",
            "EquipmentTypeId" => "Тип оборудования",
            "EquipmentStatusId" => "Статус",
            "LocationId" => "Местоположение",
            _ => fieldName
        };
    }

    private static string GetHistoryBadgeClass(string actionType)
    {
        return actionType switch
        {
            "Created" => "bg-success text-white",
            "Updated" => "bg-primary text-white",
            "StatusChanged" => "bg-warning text-dark",
            "LocationChanged" => "bg-info text-dark",
            "InventoryChecked" => "bg-dark text-white",
            "WrittenOff" => "bg-danger text-white",
            _ => "bg-secondary text-white"
        };
    }
}
