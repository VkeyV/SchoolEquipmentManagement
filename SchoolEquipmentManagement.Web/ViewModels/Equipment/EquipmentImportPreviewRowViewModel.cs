namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentImportPreviewRowViewModel
    {
        public int RowNumber { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentTypeName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public bool IsValid { get; set; }
    }
}
