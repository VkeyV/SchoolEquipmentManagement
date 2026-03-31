namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentIssueGroupViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<EquipmentIssueListItemViewModel> Items { get; set; } = new();

        public int Count => Items.Count;

        public string AccentClass => Code switch
        {
            "missing" => "bg-danger-subtle text-danger-emphasis",
            "diagnostics" => "bg-warning-subtle text-warning-emphasis",
            "discrepancy" => "bg-info-subtle text-info-emphasis",
            "repair" => "bg-secondary-subtle text-secondary-emphasis",
            _ => "bg-light text-dark"
        };
    }
}
