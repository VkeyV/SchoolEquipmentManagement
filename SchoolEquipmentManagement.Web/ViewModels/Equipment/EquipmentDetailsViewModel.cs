namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentDetailsViewModel
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? CommissioningDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public string? ResponsiblePerson { get; set; }
        public string? Notes { get; set; }
        public bool IsWrittenOff { get; set; }
        public string PhotoSource { get; set; } = string.Empty;
        public string QrCodeSource { get; set; } = string.Empty;
        public string CodeDataUri { get; set; } = string.Empty;
        public string ServiceSummary { get; set; } = string.Empty;
        public string LifecycleStage { get; set; } = string.Empty;
        public string LifecycleSummary { get; set; } = string.Empty;
        public string WarrantyStatus { get; set; } = string.Empty;
        public string WarrantySummary { get; set; } = string.Empty;
        public string WarrantyRisk { get; set; } = string.Empty;
        public string WarrantyRiskBadgeClass { get; set; } = string.Empty;
        public string WarrantyBadgeClass { get; set; } = string.Empty;
        public string LifecycleBadgeClass { get; set; } = string.Empty;
        public string LastInventorySummary { get; set; } = string.Empty;
        public string OwnershipSummary { get; set; } = string.Empty;
        public int HistoryPage { get; set; }
        public int HistoryPageSize { get; set; }
        public int HistoryTotalCount { get; set; }
        public int HistoryTotalPages { get; set; }
        public DateTime? LastChangedAt { get; set; }
        public string? LastChangedBy { get; set; }
        public bool CanEdit { get; set; }
        public bool CanChangeStatus { get; set; }
        public bool CanChangeLocation { get; set; }
        public bool CanWriteOff { get; set; }

        public List<EquipmentHistoryViewModel> History { get; set; } = new();
        public List<EquipmentMovementViewModel> Movements { get; set; } = new();

        public string DisplaySerialNumber => EquipmentDisplayFormatter.Text(SerialNumber);
        public string DisplayManufacturer => EquipmentDisplayFormatter.Text(Manufacturer);
        public string DisplayModel => EquipmentDisplayFormatter.Text(Model);
        public string DisplayPurchaseDate => EquipmentDisplayFormatter.Date(PurchaseDate);
        public string DisplayCommissioningDate => EquipmentDisplayFormatter.Date(CommissioningDate);
        public string DisplayWarrantyEndDate => EquipmentDisplayFormatter.Date(WarrantyEndDate);
        public string DisplayResponsiblePerson => EquipmentDisplayFormatter.Text(ResponsiblePerson);
        public string DisplayNotes => EquipmentDisplayFormatter.Text(Notes);
        public string DisplayLastChangedAt => EquipmentDisplayFormatter.Date(LastChangedAt);
        public string DisplayLastChangedBy => EquipmentDisplayFormatter.Text(LastChangedBy);
        public int HistoryStartItem => PaginationDisplayHelper.GetStartItem(HistoryPage, HistoryPageSize, HistoryTotalCount);
        public int HistoryEndItem => PaginationDisplayHelper.GetEndItem(HistoryPage, HistoryPageSize, HistoryTotalCount);
        public bool IsFirstHistoryPage => HistoryPage <= 1;
        public bool IsLastHistoryPage => HistoryPage >= HistoryTotalPages;
        public int PreviousHistoryPage => IsFirstHistoryPage ? 1 : HistoryPage - 1;
        public int NextHistoryPage => IsLastHistoryPage ? HistoryTotalPages : HistoryPage + 1;
    }
}
