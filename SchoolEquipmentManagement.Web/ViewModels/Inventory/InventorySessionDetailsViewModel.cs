namespace SchoolEquipmentManagement.Web.ViewModels.Inventory
{
    public class InventorySessionDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public int TotalEquipmentCount { get; set; }
        public int CheckedCount { get; set; }
        public int FoundCount { get; set; }
        public int MissingCount { get; set; }
        public int DiscrepancyCount { get; set; }
        public List<InventorySessionEquipmentItemViewModel> EquipmentItems { get; set; } = new();
        public bool CanStart => InventorySessionStatusPresentation.CanStart(Status);
        public bool CanComplete => InventorySessionStatusPresentation.CanComplete(Status);
        public bool IsReadOnly => InventorySessionStatusPresentation.IsReadOnly(Status);
        public string StatusBadgeClass => InventorySessionStatusPresentation.GetBadgeClass(Status);
        public bool CanManageSession { get; set; }
        public bool CanCheckInventory { get; set; }
    }
}
