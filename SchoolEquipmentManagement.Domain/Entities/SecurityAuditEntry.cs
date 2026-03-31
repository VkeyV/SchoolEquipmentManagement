using SchoolEquipmentManagement.Domain.Common;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Domain.Entities
{
    public class SecurityAuditEntry : BaseEntity
    {
        public SecurityAuditEventType EventType { get; private set; }
        public bool IsSuccessful { get; private set; }
        public string Summary { get; private set; }
        public string? UserName { get; private set; }
        public string? TargetUserName { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public DateTime OccurredAt { get; private set; }

        private SecurityAuditEntry()
        {
            Summary = null!;
        }

        public SecurityAuditEntry(
            SecurityAuditEventType eventType,
            bool isSuccessful,
            string summary,
            string? userName = null,
            string? targetUserName = null,
            string? ipAddress = null,
            string? userAgent = null)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                throw new DomainException("Не указано описание события безопасности.");
            }

            EventType = eventType;
            IsSuccessful = isSuccessful;
            Summary = summary.Trim();
            UserName = Normalize(userName);
            TargetUserName = Normalize(targetUserName);
            IpAddress = Normalize(ipAddress);
            UserAgent = Normalize(userAgent);
            OccurredAt = DateTime.UtcNow;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
