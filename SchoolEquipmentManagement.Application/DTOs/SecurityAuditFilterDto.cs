namespace SchoolEquipmentManagement.Application.DTOs
{
    public class SecurityAuditFilterDto
    {
        public string? Search { get; set; }
        public bool FailuresOnly { get; set; }
        public int Take { get; set; } = 100;
    }
}
