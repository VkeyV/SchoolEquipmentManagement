namespace SchoolEquipmentManagement.Application.DTOs
{
    public class LocationDetailsDto
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
        public List<LocationStatusSummaryDto> StatusSummary { get; set; } = new();
        public List<LocationEquipmentItemDto> EquipmentItems { get; set; } = new();
        public List<LocationInventoryDiscrepancyDto> InventoryDiscrepancies { get; set; } = new();
    }
}
