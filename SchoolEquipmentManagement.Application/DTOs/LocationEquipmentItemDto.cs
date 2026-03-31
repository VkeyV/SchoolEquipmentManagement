namespace SchoolEquipmentManagement.Application.DTOs
{
    public class LocationEquipmentItemDto
    {
        public int Id { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentTypeName { get; set; } = string.Empty;
        public string EquipmentStatusName { get; set; } = string.Empty;
        public string? ResponsiblePerson { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public int? WarrantyDaysLeft { get; set; }
        public string LastInventoryStatus { get; set; } = string.Empty;
        public DateTime? LastInventoryCheckedAt { get; set; }
        public string? LastInventoryCheckedBy { get; set; }
    }
}
