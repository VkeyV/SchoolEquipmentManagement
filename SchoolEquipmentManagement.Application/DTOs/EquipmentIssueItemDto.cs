namespace SchoolEquipmentManagement.Application.DTOs
{
    public class EquipmentIssueItemDto
    {
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentTypeName { get; set; } = string.Empty;
        public string EquipmentStatusName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string? ResponsiblePerson { get; set; }
        public string IssueCode { get; set; } = string.Empty;
        public string IssueTitle { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string PriorityLabel { get; set; } = string.Empty;
        public DateTime? LastCheckedAt { get; set; }
        public string? LastCheckedBy { get; set; }
        public string? ExpectedLocationName { get; set; }
        public string? ActualLocationName { get; set; }
    }
}
