namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    internal static class EquipmentStatusPresentation
    {
        private const string WrittenOffStatus = "\u0421\u043F\u0438\u0441\u0430\u043D\u043E";
        private const string RepairStatus = "\u0412 \u0440\u0435\u043C\u043E\u043D\u0442\u0435";
        private const string InUseStatus = "\u0412 \u044D\u043A\u0441\u043F\u043B\u0443\u0430\u0442\u0430\u0446\u0438\u0438";
        private const string WarehouseStatus = "\u041D\u0430 \u0441\u043A\u043B\u0430\u0434\u0435";
        private const string ReserveStatus = "\u0412 \u0440\u0435\u0437\u0435\u0440\u0432\u0435";
        private const string DiagnosticsStatus = "\u0422\u0440\u0435\u0431\u0443\u0435\u0442 \u0434\u0438\u0430\u0433\u043D\u043E\u0441\u0442\u0438\u043A\u0438";

        public static string GetBadgeClass(string status) => status switch
        {
            WrittenOffStatus => "bg-danger text-white",
            RepairStatus => "bg-warning text-dark",
            DiagnosticsStatus => "bg-warning-subtle text-warning-emphasis",
            InUseStatus => "bg-success text-white",
            WarehouseStatus => "bg-secondary text-white",
            ReserveStatus => "bg-info text-dark",
            _ => "bg-light text-dark border"
        };
    }
}
