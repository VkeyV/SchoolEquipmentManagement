namespace SchoolEquipmentManagement.Application.DTOs
{
    public class InventorySessionEquipmentItemDto
    {
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentTypeName { get; set; } = string.Empty;
        public string ExpectedLocationName { get; set; } = string.Empty;
        public string EquipmentStatusName { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
        public bool? IsFound { get; set; }
        public int? ActualLocationId { get; set; }
        public string? ActualLocationName { get; set; }
        public string? ConditionComment { get; set; }
        public DateTime? CheckedAt { get; set; }
        public string? CheckedBy { get; set; }
        public bool HasLocationDiscrepancy { get; set; }
    }
}
