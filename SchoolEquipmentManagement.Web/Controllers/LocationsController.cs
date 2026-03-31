using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.ViewModels.Locations;

namespace SchoolEquipmentManagement.Web.Controllers;

[Authorize]
public class LocationsController : AppController
{
    private readonly IEquipmentService _equipmentService;
    private readonly IUserAccessService _userAccessService;

    public LocationsController(IEquipmentService equipmentService, IUserAccessService userAccessService)
    {
        _equipmentService = equipmentService;
        _userAccessService = userAccessService;
    }

    [PermissionAuthorize(ModulePermission.ViewEquipment)]
    public async Task<IActionResult> Details(int id)
    {
        var location = await _equipmentService.GetLocationDetailsAsync(id);
        if (location is null)
        {
            return NotFound();
        }

        return View(CreateViewModel(location));
    }

    private LocationDetailsViewModel CreateViewModel(LocationDetailsDto location)
    {
        var canChangeStatus = _userAccessService.HasPermission(ModulePermission.ChangeEquipmentStatus);
        var canAssignResponsible = _userAccessService.HasPermission(ModulePermission.EditEquipment);

        var equipmentItems = location.EquipmentItems
            .Select(item => new LocationEquipmentItemViewModel
            {
                Id = item.Id,
                InventoryNumber = item.InventoryNumber,
                Name = item.Name,
                EquipmentType = item.EquipmentTypeName,
                Status = item.EquipmentStatusName,
                ResponsiblePerson = item.ResponsiblePerson,
                WarrantyEndDate = item.WarrantyEndDate,
                WarrantyDaysLeft = item.WarrantyDaysLeft,
                LastInventoryStatus = item.LastInventoryStatus,
                LastInventoryCheckedAt = item.LastInventoryCheckedAt,
                LastInventoryCheckedBy = item.LastInventoryCheckedBy
            })
            .ToList();

        var inventoryDiscrepancies = location.InventoryDiscrepancies
            .Select(item => new LocationInventoryDiscrepancyViewModel
            {
                EquipmentId = item.EquipmentId,
                InventoryNumber = item.InventoryNumber,
                Name = item.Name,
                EquipmentType = item.EquipmentTypeName,
                Status = item.EquipmentStatusName,
                ExpectedLocation = item.ExpectedLocationName,
                ActualLocation = item.ActualLocationName,
                DiscrepancyCode = item.DiscrepancyCode,
                DiscrepancyTitle = item.DiscrepancyTitle,
                DiscrepancySummary = item.DiscrepancySummary,
                CheckedAt = item.CheckedAt,
                CheckedBy = item.CheckedBy,
                ConditionComment = item.ConditionComment
            })
            .ToList();

        var planTemplate = ResolvePlanTemplate(location);

        return new LocationDetailsViewModel
        {
            Id = location.Id,
            Name = location.Name,
            Building = location.Building,
            Room = location.Room,
            Description = location.Description,
            EquipmentCount = location.EquipmentCount,
            DiscrepancyCount = location.DiscrepancyCount,
            MissingCount = location.MissingCount,
            LastInventoryCheckedAt = location.LastInventoryCheckedAt,
            CanChangeStatus = canChangeStatus,
            CanAssignResponsible = canAssignResponsible,
            StatusSummary = location.StatusSummary
                .Select(item => new LocationStatusSummaryViewModel
                {
                    StatusName = item.StatusName,
                    Count = item.Count
                })
                .ToList(),
            EquipmentItems = equipmentItems,
            InventoryDiscrepancies = inventoryDiscrepancies,
            MapZones = planTemplate.Zones
                .Select(zone => new LocationMapZoneViewModel
                {
                    Code = zone.Code,
                    Title = zone.Title,
                    Caption = zone.Caption,
                    LeftPercent = zone.LeftPercent,
                    TopPercent = zone.TopPercent,
                    WidthPercent = zone.WidthPercent,
                    HeightPercent = zone.HeightPercent,
                    AccentClass = zone.AccentClass
                })
                .ToList(),
            MapMarkers = BuildMapMarkers(planTemplate, equipmentItems, inventoryDiscrepancies, canChangeStatus, canAssignResponsible)
        };
    }

    private List<LocationMapMarkerViewModel> BuildMapMarkers(
        LocationPlanTemplate planTemplate,
        IReadOnlyList<LocationEquipmentItemViewModel> equipmentItems,
        IReadOnlyList<LocationInventoryDiscrepancyViewModel> discrepancies,
        bool canChangeStatus,
        bool canAssignResponsible)
    {
        var discrepanciesByEquipmentId = discrepancies
            .GroupBy(item => item.EquipmentId)
            .ToDictionary(group => group.Key, group => group.First());

        var seeds = new List<LocationMarkerSeed>();

        foreach (var equipment in equipmentItems)
        {
            discrepanciesByEquipmentId.TryGetValue(equipment.Id, out var discrepancy);

            seeds.Add(new LocationMarkerSeed
            {
                EquipmentId = equipment.Id,
                InventoryNumber = equipment.InventoryNumber,
                Name = equipment.Name,
                EquipmentType = equipment.EquipmentType,
                Status = equipment.Status,
                ResponsiblePerson = equipment.ResponsiblePerson,
                ZoneCode = ResolveZoneCode(planTemplate.Kind, equipment, discrepancy),
                MarkerClass = ResolveMarkerClass(equipment.Status, discrepancy),
                StateLabel = ResolveStateLabel(equipment.Status, discrepancy),
                Summary = ResolveMarkerSummary(equipment, discrepancy),
                DiscrepancyTitle = discrepancy?.DiscrepancyTitle,
                ExpectedLocation = discrepancy?.ExpectedLocation,
                ActualLocation = discrepancy?.ActualLocation,
                IsDiscrepancy = discrepancy is not null
            });
        }

        foreach (var discrepancy in discrepancies.Where(item => item.DiscrepancyCode == "moved-in" && equipmentItems.All(equipment => equipment.Id != item.EquipmentId)))
        {
            var placeholderEquipment = new LocationEquipmentItemViewModel
            {
                Id = discrepancy.EquipmentId,
                InventoryNumber = discrepancy.InventoryNumber,
                Name = discrepancy.Name,
                EquipmentType = discrepancy.EquipmentType,
                Status = discrepancy.Status
            };

            seeds.Add(new LocationMarkerSeed
            {
                EquipmentId = discrepancy.EquipmentId,
                InventoryNumber = discrepancy.InventoryNumber,
                Name = discrepancy.Name,
                EquipmentType = discrepancy.EquipmentType,
                Status = discrepancy.Status,
                ZoneCode = ResolveZoneCode(planTemplate.Kind, placeholderEquipment, discrepancy),
                MarkerClass = ResolveMarkerClass(discrepancy.Status, discrepancy),
                StateLabel = ResolveStateLabel(discrepancy.Status, discrepancy),
                Summary = discrepancy.DiscrepancySummary,
                DiscrepancyTitle = discrepancy.DiscrepancyTitle,
                ExpectedLocation = discrepancy.ExpectedLocation,
                ActualLocation = discrepancy.ActualLocation,
                IsDiscrepancy = true
            });
        }

        var markers = new List<LocationMapMarkerViewModel>();

        foreach (var zoneGroup in seeds.GroupBy(seed => seed.ZoneCode))
        {
            var zone = planTemplate.Zones.FirstOrDefault(item => item.Code == zoneGroup.Key) ?? planTemplate.Zones[0];
            var orderedSeeds = zoneGroup
                .OrderByDescending(item => item.IsDiscrepancy)
                .ThenBy(item => item.InventoryNumber)
                .ToList();

            var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(orderedSeeds.Count)));
            var rows = Math.Max(1, (int)Math.Ceiling((double)orderedSeeds.Count / columns));

            for (var index = 0; index < orderedSeeds.Count; index++)
            {
                var row = index / columns;
                var column = index % columns;
                var left = zone.LeftPercent + zone.WidthPercent * ((column + 1d) / (columns + 1d));
                var top = zone.TopPercent + zone.HeightPercent * ((row + 1d) / (rows + 1d));
                var seed = orderedSeeds[index];

                markers.Add(new LocationMapMarkerViewModel
                {
                    EquipmentId = seed.EquipmentId,
                    InventoryNumber = seed.InventoryNumber,
                    Name = seed.Name,
                    EquipmentType = seed.EquipmentType,
                    Status = seed.Status,
                    ResponsiblePerson = seed.ResponsiblePerson,
                    ZoneCode = zone.Code,
                    LeftPercent = left,
                    TopPercent = top,
                    MarkerClass = seed.MarkerClass,
                    StateLabel = seed.StateLabel,
                    ShortLabel = BuildShortLabel(seed.InventoryNumber),
                    Summary = seed.Summary,
                    DiscrepancyTitle = seed.DiscrepancyTitle,
                    ExpectedLocation = seed.ExpectedLocation,
                    ActualLocation = seed.ActualLocation,
                    IsDiscrepancy = seed.IsDiscrepancy,
                    CanChangeStatus = canChangeStatus,
                    CanAssignResponsible = canAssignResponsible
                });
            }
        }

        return markers
            .OrderBy(item => item.ZoneCode)
            .ThenBy(item => item.InventoryNumber)
            .ToList();
    }

    private static LocationPlanTemplate ResolvePlanTemplate(LocationDetailsDto location)
    {
        var building = location.Building.ToLowerInvariant();
        var room = location.Room.ToLowerInvariant();

        if (room.Contains("сервер"))
        {
            return new LocationPlanTemplate(
                "server",
                new List<LocationPlanZoneTemplate>
                {
                    new("rack-west", "Стойки A", "Основное серверное оборудование", 8, 10, 26, 48, "accent-blue"),
                    new("rack-east", "Стойки B", "Сетевой и резервный контур", 38, 10, 26, 48, "accent-indigo"),
                    new("service", "Сервисная зона", "Диагностика и обслуживание", 68, 10, 24, 22, "accent-amber"),
                    new("reserve", "Резерв", "Хранение и подготовка", 68, 36, 24, 22, "accent-emerald")
                });
        }

        if (building.Contains("склад") || room.Contains("склад"))
        {
            return new LocationPlanTemplate(
                "warehouse",
                new List<LocationPlanZoneTemplate>
                {
                    new("intake", "Приемка", "Новые и проблемные позиции", 8, 10, 20, 18, "accent-amber"),
                    new("shelf-a", "Стеллаж A", "Основной запас", 32, 10, 24, 48, "accent-blue"),
                    new("shelf-b", "Стеллаж B", "Резерв и периферия", 60, 10, 24, 48, "accent-indigo"),
                    new("reserve", "Резерв", "Подготовка к выдаче", 8, 34, 20, 24, "accent-emerald")
                });
        }

        return new LocationPlanTemplate(
            "room",
            new List<LocationPlanZoneTemplate>
            {
                new("instructor", "Рабочее место", "Зона преподавателя и ответственных", 6, 8, 22, 18, "accent-blue"),
                new("workspace", "Класс", "Основные рабочие места", 32, 10, 42, 46, "accent-indigo"),
                new("service", "Хранение и сервис", "Резерв, ремонт и временное размещение", 6, 34, 22, 22, "accent-amber"),
                new("periphery", "Периферия", "Проекторы, печать и сеть", 78, 10, 16, 46, "accent-emerald")
            });
    }

    private static string ResolveZoneCode(
        string planKind,
        LocationEquipmentItemViewModel equipment,
        LocationInventoryDiscrepancyViewModel? discrepancy)
    {
        var equipmentType = equipment.EquipmentType.ToLowerInvariant();
        var status = equipment.Status.ToLowerInvariant();
        var name = equipment.Name.ToLowerInvariant();

        if (planKind == "server")
        {
            if (discrepancy?.DiscrepancyCode == "missing" || status.Contains("ремонт") || status.Contains("диагност"))
            {
                return "service";
            }

            if (status.Contains("резерв") || status.Contains("склад"))
            {
                return "reserve";
            }

            return equipmentType.Contains("маршрутизатор") || equipmentType.Contains("панель")
                ? "rack-east"
                : "rack-west";
        }

        if (planKind == "warehouse")
        {
            if (discrepancy is not null || status.Contains("ремонт") || status.Contains("диагност"))
            {
                return "intake";
            }

            if (status.Contains("резерв") || status.Contains("склад"))
            {
                return "reserve";
            }

            return equipmentType.Contains("маршрутизатор") || equipmentType.Contains("принтер") || equipmentType.Contains("проектор")
                ? "shelf-b"
                : "shelf-a";
        }

        if (discrepancy is not null || status.Contains("ремонт") || status.Contains("диагност") || status.Contains("резерв") || status.Contains("склад"))
        {
            return "service";
        }

        if (name.Contains("учитель") || name.Contains("директор") || name.Contains("руководител") || name.Contains("администрац"))
        {
            return "instructor";
        }

        if (equipmentType.Contains("принтер") || equipmentType.Contains("проектор") || equipmentType.Contains("маршрутизатор") || equipmentType.Contains("панель"))
        {
            return "periphery";
        }

        return "workspace";
    }

    private static string ResolveMarkerClass(string status, LocationInventoryDiscrepancyViewModel? discrepancy)
    {
        var normalizedStatus = status.ToLowerInvariant();

        if (discrepancy?.DiscrepancyCode == "missing")
        {
            return "is-missing";
        }

        if (discrepancy?.DiscrepancyCode == "moved-out")
        {
            return "is-warning";
        }

        if (discrepancy?.DiscrepancyCode == "moved-in")
        {
            return "is-discovered";
        }

        if (normalizedStatus.Contains("ремонт") || normalizedStatus.Contains("диагност"))
        {
            return "is-service";
        }

        if (normalizedStatus.Contains("резерв") || normalizedStatus.Contains("склад"))
        {
            return "is-storage";
        }

        return "is-active";
    }

    private static string ResolveStateLabel(string status, LocationInventoryDiscrepancyViewModel? discrepancy)
    {
        if (discrepancy is not null)
        {
            return discrepancy.DiscrepancyTitle;
        }

        return status;
    }

    private static string ResolveMarkerSummary(LocationEquipmentItemViewModel equipment, LocationInventoryDiscrepancyViewModel? discrepancy)
    {
        if (discrepancy is not null)
        {
            return discrepancy.DiscrepancySummary;
        }

        return equipment.LastInventoryStatus;
    }

    private static string BuildShortLabel(string inventoryNumber)
    {
        var digits = new string(inventoryNumber.Where(char.IsDigit).ToArray());
        if (digits.Length >= 2)
        {
            return digits[^2..];
        }

        return inventoryNumber.Length >= 2
            ? inventoryNumber[^2..].ToUpperInvariant()
            : inventoryNumber.ToUpperInvariant();
    }

    private sealed class LocationMarkerSeed
    {
        public int EquipmentId { get; init; }
        public string InventoryNumber { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string EquipmentType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? ResponsiblePerson { get; init; }
        public string ZoneCode { get; init; } = string.Empty;
        public string MarkerClass { get; init; } = string.Empty;
        public string StateLabel { get; init; } = string.Empty;
        public string Summary { get; init; } = string.Empty;
        public string? DiscrepancyTitle { get; init; }
        public string? ExpectedLocation { get; init; }
        public string? ActualLocation { get; init; }
        public bool IsDiscrepancy { get; init; }
    }

    private sealed record LocationPlanTemplate(string Kind, List<LocationPlanZoneTemplate> Zones);

    private sealed record LocationPlanZoneTemplate(
        string Code,
        string Title,
        string Caption,
        double LeftPercent,
        double TopPercent,
        double WidthPercent,
        double HeightPercent,
        string AccentClass);
}
