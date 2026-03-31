namespace SchoolEquipmentManagement.Web.ViewModels.Locations
{
    public class LocationDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EquipmentCount { get; set; }
        public int DiscrepancyCount { get; set; }
        public int MissingCount { get; set; }
        public DateTime? LastInventoryCheckedAt { get; set; }
        public bool CanChangeStatus { get; set; }
        public bool CanAssignResponsible { get; set; }
        public List<LocationStatusSummaryViewModel> StatusSummary { get; set; } = new();
        public List<LocationEquipmentItemViewModel> EquipmentItems { get; set; } = new();
        public List<LocationInventoryDiscrepancyViewModel> InventoryDiscrepancies { get; set; } = new();
        public List<LocationMapZoneViewModel> MapZones { get; set; } = new();
        public List<LocationMapMarkerViewModel> MapMarkers { get; set; } = new();

        public bool HasEquipment => EquipmentItems.Count > 0;
        public bool HasInventoryDiscrepancies => InventoryDiscrepancies.Count > 0;
        public bool HasInteractiveMap => MapMarkers.Count > 0;
        public string DisplayDescription => string.IsNullOrWhiteSpace(Description) ? "Описание не заполнено." : Description!;
        public string DisplayLastInventoryCheckedAt => LastInventoryCheckedAt.HasValue ? LastInventoryCheckedAt.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm") : "Нет проверок";
    }
}
