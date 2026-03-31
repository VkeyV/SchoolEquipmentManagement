namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentWarrantyReportItemViewModel
    {
        public int Id { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? ResponsiblePerson { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public int? WarrantyDaysLeft { get; set; }

        public string StatusBadgeClass => EquipmentStatusPresentation.GetBadgeClass(Status);
        public string WarrantyRisk => EquipmentWarrantyPresentation.GetRiskLabel(WarrantyEndDate);
        public string WarrantyRiskBadgeClass => EquipmentWarrantyPresentation.GetRiskBadgeClass(WarrantyRisk);
        public string WarrantySummary => EquipmentWarrantyPresentation.GetSummary(WarrantyEndDate);
        public string DisplayResponsiblePerson => EquipmentDisplayFormatter.Text(ResponsiblePerson);
        public string DisplayWarrantyEndDate => EquipmentDisplayFormatter.Date(WarrantyEndDate);
    }
}
