namespace SchoolEquipmentManagement.Application.DTOs
{
    public class EquipmentImportPreviewRowDto
    {
        public int RowNumber { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentTypeName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public bool IsValid => Errors.Count == 0;
    }
}
