using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class SecurityAuditListItemDto
    {
        public DateTime OccurredAt { get; set; }
        public SecurityAuditEventType EventType { get; set; }
        public bool IsSuccessful { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? TargetUserName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
