using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.ViewModels.Locations
{
    public class LocationEquipmentItemViewModel
    {
        public int Id { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ResponsiblePerson { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public int? WarrantyDaysLeft { get; set; }
        public string LastInventoryStatus { get; set; } = string.Empty;
        public DateTime? LastInventoryCheckedAt { get; set; }
        public string? LastInventoryCheckedBy { get; set; }

        public string StatusBadgeClass => EquipmentStatusPresentation.GetBadgeClass(Status);
        public string WarrantyRisk => EquipmentWarrantyPresentation.GetRiskLabel(WarrantyEndDate);
        public string WarrantyRiskBadgeClass => EquipmentWarrantyPresentation.GetRiskBadgeClass(WarrantyRisk);
        public string DisplayWarrantyEndDate => EquipmentDisplayFormatter.Date(WarrantyEndDate);
        public string DisplayResponsiblePerson => EquipmentDisplayFormatter.Text(ResponsiblePerson);
        public string DisplayLastInventoryCheckedAt => EquipmentDisplayFormatter.Date(LastInventoryCheckedAt);
        public string DisplayLastInventoryCheckedBy => EquipmentDisplayFormatter.Text(LastInventoryCheckedBy);
    }
}
