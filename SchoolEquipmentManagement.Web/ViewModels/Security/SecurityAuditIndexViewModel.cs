namespace SchoolEquipmentManagement.Web.ViewModels.Security
{
    public class SecurityAuditIndexViewModel
    {
        public string? Search { get; set; }
        public bool FailuresOnly { get; set; }
        public List<SecurityAuditItemViewModel> Items { get; set; } = new();
        public int TotalCount => Items.Count;
        public int FailureCount => Items.Count(x => !x.IsSuccessful);
        public int SuccessCount => Items.Count(x => x.IsSuccessful);
    }
}
