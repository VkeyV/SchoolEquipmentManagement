namespace SchoolEquipmentManagement.Application.DTOs
{
    public class CreateInventorySessionDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string CreatedBy { get; set; } = "System";
    }
}
