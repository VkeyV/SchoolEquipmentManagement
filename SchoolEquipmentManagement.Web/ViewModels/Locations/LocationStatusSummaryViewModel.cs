using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.ViewModels.Locations
{
    public class LocationStatusSummaryViewModel
    {
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }

        public string BadgeClass => EquipmentStatusPresentation.GetBadgeClass(StatusName);
    }
}
