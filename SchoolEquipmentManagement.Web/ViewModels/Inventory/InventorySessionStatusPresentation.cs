namespace SchoolEquipmentManagement.Web.ViewModels.Inventory
{
    internal static class InventorySessionStatusPresentation
    {
        private const string DraftStatus = "\u0427\u0435\u0440\u043D\u043E\u0432\u0438\u043A";
        private const string ActiveStatus = "\u0410\u043A\u0442\u0438\u0432\u043D\u0430";
        private const string CompletedStatus = "\u0417\u0430\u0432\u0435\u0440\u0448\u0435\u043D\u0430";
        private const string CancelledStatus = "\u041E\u0442\u043C\u0435\u043D\u0435\u043D\u0430";

        public static bool CanStart(string status) => status == DraftStatus;

        public static bool CanComplete(string status) => status == ActiveStatus;

        public static bool IsReadOnly(string status) =>
            status == CompletedStatus || status == CancelledStatus;

        public static string GetBadgeClass(string status) => status switch
        {
            DraftStatus => "bg-secondary text-white",
            ActiveStatus => "bg-primary text-white",
            CompletedStatus => "bg-success text-white",
            _ => "bg-light text-dark border"
        };
    }
}
