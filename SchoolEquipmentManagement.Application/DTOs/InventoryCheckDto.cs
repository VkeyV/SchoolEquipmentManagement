namespace SchoolEquipmentManagement.Application.DTOs
{
    public class InventoryCheckDto
    {
        public int SessionId { get; set; }
        public int EquipmentId { get; set; }
        public bool IsFound { get; set; }
        public int? ActualLocationId { get; set; }
        public string? ConditionComment { get; set; }
        public string CheckedBy { get; set; } = "System";
    }
}
