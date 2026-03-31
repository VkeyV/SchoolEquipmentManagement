namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    internal static class EquipmentWarrantyPresentation
    {
        public static string GetRiskLabel(DateTime? warrantyEndDate)
        {
            var daysLeft = GetDaysLeft(warrantyEndDate);

            if (!daysLeft.HasValue)
            {
                return "Нет данных";
            }

            if (daysLeft.Value < 0)
            {
                return "Критичный";
            }

            if (daysLeft.Value <= 30)
            {
                return "Высокий";
            }

            if (daysLeft.Value <= 60)
            {
                return "Средний";
            }

            if (daysLeft.Value <= 90)
            {
                return "Низкий";
            }

            return "Плановый";
        }

        public static string GetRiskBadgeClass(string riskLabel) => riskLabel switch
        {
            "Критичный" => "bg-danger text-white",
            "Высокий" => "bg-warning text-dark",
            "Средний" => "bg-primary text-white",
            "Низкий" => "bg-info text-dark",
            "Плановый" => "bg-success text-white",
            _ => "bg-secondary text-white"
        };

        public static string GetSummary(DateTime? warrantyEndDate)
        {
            var daysLeft = GetDaysLeft(warrantyEndDate);

            if (!daysLeft.HasValue)
            {
                return "Дата окончания гарантии не указана.";
            }

            if (daysLeft.Value < 0)
            {
                return $"Гарантия истекла {warrantyEndDate:dd.MM.yyyy}.";
            }

            if (daysLeft.Value == 0)
            {
                return "Гарантия истекает сегодня.";
            }

            return $"До окончания гарантии осталось {daysLeft.Value} дн.";
        }

        public static int? GetDaysLeft(DateTime? warrantyEndDate)
        {
            return warrantyEndDate.HasValue
                ? (warrantyEndDate.Value.Date - DateTime.Today).Days
                : null;
        }
    }
}
