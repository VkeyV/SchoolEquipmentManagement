namespace SchoolEquipmentManagement.Application.DTOs
{
    public class AssignEquipmentResponsibleDto
    {
        public int EquipmentId { get; set; }
        public string? ResponsiblePerson { get; set; }
        public string? Comment { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
    }
}
