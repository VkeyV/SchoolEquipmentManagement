namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentHistoryViewModel
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? ChangedField { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Comment { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string BadgeClass { get; set; } = "text-bg-secondary";

        public string DisplayChangedField => EquipmentDisplayFormatter.Text(ChangedField);
        public string DisplayOldValue => EquipmentDisplayFormatter.Text(OldValue);
        public string DisplayNewValue => EquipmentDisplayFormatter.Text(NewValue);
        public string DisplayComment => EquipmentDisplayFormatter.Text(Comment);
        public string DisplayChangedBy => EquipmentDisplayFormatter.Text(ChangedBy);
        public string DisplayChangedAt => EquipmentDisplayFormatter.LocalDateTime(ChangedAt);
    }
}
