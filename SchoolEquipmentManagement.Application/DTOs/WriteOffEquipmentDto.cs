namespace SchoolEquipmentManagement.Application.DTOs
{
    public class WriteOffEquipmentDto
    {
        public int EquipmentId { get; set; }
        public int WrittenOffStatusId { get; set; }
        public string ChangedBy { get; set; } = "System";
        public string? Comment { get; set; }
    }
}
