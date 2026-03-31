namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    internal static class EquipmentIssuePresentation
    {
        public static string GetPriorityBadgeClass(string priorityLabel) => priorityLabel switch
        {
            "Критичный" => "bg-danger text-white",
            "Высокий" => "bg-warning text-dark",
            "Средний" => "bg-primary text-white",
            _ => "bg-light text-dark border"
        };
    }
}
