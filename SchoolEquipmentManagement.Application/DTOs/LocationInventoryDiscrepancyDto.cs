namespace SchoolEquipmentManagement.Application.DTOs
{
    public class LocationInventoryDiscrepancyDto
    {
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentTypeName { get; set; } = string.Empty;
        public string EquipmentStatusName { get; set; } = string.Empty;
        public string ExpectedLocationName { get; set; } = string.Empty;
        public string? ActualLocationName { get; set; }
        public string DiscrepancyCode { get; set; } = string.Empty;
        public string DiscrepancyTitle { get; set; } = string.Empty;
        public string DiscrepancySummary { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
        public string CheckedBy { get; set; } = string.Empty;
        public string? ConditionComment { get; set; }
    }
}
