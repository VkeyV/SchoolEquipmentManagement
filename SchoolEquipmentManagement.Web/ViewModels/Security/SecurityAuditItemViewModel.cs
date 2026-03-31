namespace SchoolEquipmentManagement.Web.ViewModels.Security
{
    public class SecurityAuditItemViewModel
    {
        public DateTime OccurredAt { get; set; }
        public string EventDisplayName { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? TargetUserName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
