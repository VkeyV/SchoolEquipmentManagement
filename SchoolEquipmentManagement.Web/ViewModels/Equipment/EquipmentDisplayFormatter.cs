namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    internal static class EquipmentDisplayFormatter
    {
        private const string MissingText = "\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E";

        public static string Text(string? value) =>
            string.IsNullOrWhiteSpace(value) ? MissingText : value;

        public static string Date(DateTime? value) =>
            value.HasValue ? value.Value.ToString("dd.MM.yyyy") : MissingText;

        public static string LocalDateTime(DateTime value) =>
            value.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    }
}
