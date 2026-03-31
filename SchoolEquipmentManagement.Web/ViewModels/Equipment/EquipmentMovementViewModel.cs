namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentMovementViewModel
    {
        public DateTime OccurredAt { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? ChangedBy { get; set; }
        public string BadgeClass { get; set; } = "bg-secondary text-white";

        public bool HasDetails => !string.IsNullOrWhiteSpace(Details);
        public string DisplayDetails => EquipmentDisplayFormatter.Text(Details);
        public string DisplayChangedBy => EquipmentDisplayFormatter.Text(ChangedBy);
        public string DisplayOccurredAt => EquipmentDisplayFormatter.LocalDateTime(OccurredAt);
    }
}
