

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class EquipmentListItemDto
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentTypeName { get; set; } = string.Empty;
        public string EquipmentStatusName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string? ResponsiblePerson { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public int? WarrantyDaysLeft { get; set; }
    }
}
