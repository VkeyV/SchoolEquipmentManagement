namespace SchoolEquipmentManagement.Application.DTOs
{
    public class InventorySessionDetailsDto
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
        public List<InventorySessionEquipmentItemDto> EquipmentItems { get; set; } = new();
    }
}
