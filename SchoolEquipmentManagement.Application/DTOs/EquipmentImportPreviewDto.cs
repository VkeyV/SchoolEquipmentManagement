namespace SchoolEquipmentManagement.Application.DTOs
{
    public class EquipmentImportPreviewDto
    {
        public List<EquipmentImportPreviewRowDto> Rows { get; set; } = new();
        public List<EquipmentImportApplyItemDto> ValidItems { get; set; } = new();
        public int TotalRows => Rows.Count;
        public int ValidRows => Rows.Count(x => x.IsValid);
        public int InvalidRows => Rows.Count(x => !x.IsValid);
    }
}
