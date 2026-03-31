namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentIssueListItemViewModel
    {
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? ResponsiblePerson { get; set; }
        public string IssueDescription { get; set; } = string.Empty;
        public string PriorityLabel { get; set; } = string.Empty;
        public DateTime? LastCheckedAt { get; set; }
        public string? LastCheckedBy { get; set; }
        public string? ActualLocationName { get; set; }
        public bool CanChangeStatus { get; set; }
        public bool CanAssignResponsible { get; set; }

        public string StatusBadgeClass => EquipmentStatusPresentation.GetBadgeClass(Status);
        public string PriorityBadgeClass => EquipmentIssuePresentation.GetPriorityBadgeClass(PriorityLabel);
        public string DisplayResponsiblePerson => EquipmentDisplayFormatter.Text(ResponsiblePerson);
        public string DisplayLastCheckedAt => LastCheckedAt.HasValue ? EquipmentDisplayFormatter.LocalDateTime(LastCheckedAt.Value) : "Не проверялось";
        public string DisplayLastCheckedBy => EquipmentDisplayFormatter.Text(LastCheckedBy);
        public string DisplayActualLocation => EquipmentDisplayFormatter.Text(ActualLocationName);
    }
}
