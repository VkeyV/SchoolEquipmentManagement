using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.ViewModels.Locations
{
    public class LocationInventoryDiscrepancyViewModel
    {
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ExpectedLocation { get; set; } = string.Empty;
        public string? ActualLocation { get; set; }
        public string DiscrepancyCode { get; set; } = string.Empty;
        public string DiscrepancyTitle { get; set; } = string.Empty;
        public string DiscrepancySummary { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
        public string CheckedBy { get; set; } = string.Empty;
        public string? ConditionComment { get; set; }

        public string StatusBadgeClass => EquipmentStatusPresentation.GetBadgeClass(Status);
        public string DiscrepancyBadgeClass => DiscrepancyCode switch
        {
            "missing" => "bg-danger-subtle text-danger-emphasis",
            "moved-out" => "bg-warning-subtle text-warning-emphasis",
            _ => "bg-info-subtle text-info-emphasis"
        };
        public string DisplayActualLocation => EquipmentDisplayFormatter.Text(ActualLocation);
        public string DisplayConditionComment => EquipmentDisplayFormatter.Text(ConditionComment);
        public string DisplayCheckedAt => CheckedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string DisplayCheckedBy => EquipmentDisplayFormatter.Text(CheckedBy);
    }
}
