namespace SchoolEquipmentManagement.Web.ViewModels.Inventory
{
    public class InventorySessionEquipmentItemViewModel
    {
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentTypeName { get; set; } = string.Empty;
        public string ExpectedLocationName { get; set; } = string.Empty;
        public string EquipmentStatusName { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
        public bool? IsFound { get; set; }
        public string? ActualLocationName { get; set; }
        public string? ConditionComment { get; set; }
        public DateTime? CheckedAt { get; set; }
        public string? CheckedBy { get; set; }
        public bool HasLocationDiscrepancy { get; set; }

        public bool HasActualLocationNote =>
            !string.IsNullOrWhiteSpace(ActualLocationName) &&
            ActualLocationName != ExpectedLocationName;

        public string ResultText => IsChecked
            ? IsFound == true
                ? HasLocationDiscrepancy ? "\u041D\u0430\u0439\u0434\u0435\u043D\u043E \u0441 \u0440\u0430\u0441\u0445\u043E\u0436\u0434\u0435\u043D\u0438\u0435\u043C" : "\u041D\u0430\u0439\u0434\u0435\u043D\u043E"
                : "\u041D\u0435 \u043D\u0430\u0439\u0434\u0435\u043D\u043E"
            : "\u041D\u0435 \u043F\u0440\u043E\u0432\u0435\u0440\u0435\u043D\u043E";

        public string ResultBadgeClass => IsChecked
            ? IsFound == true
                ? HasLocationDiscrepancy ? "bg-warning text-dark" : "bg-success text-white"
                : "bg-danger text-white"
            : "bg-secondary text-white";

        public string DisplayConditionComment =>
            string.IsNullOrWhiteSpace(ConditionComment) ? "\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E" : ConditionComment;

        public string CheckActionText => IsChecked
            ? "\u0418\u0437\u043C\u0435\u043D\u0438\u0442\u044C"
            : "\u041F\u0440\u043E\u0432\u0435\u0440\u0438\u0442\u044C";
    }
}
