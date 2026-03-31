namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentIssuesIndexViewModel
    {
        public List<EquipmentIssueGroupViewModel> Groups { get; set; } = new();

        public int TotalIssues => Groups.Sum(group => group.Count);

        public bool HasIssues => TotalIssues > 0;
    }
}
