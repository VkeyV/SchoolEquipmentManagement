using Microsoft.AspNetCore.Http;

namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentImportViewModel
    {
        public IFormFile? File { get; set; }
        public bool HasPreview { get; set; }
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public string PayloadJson { get; set; } = string.Empty;
        public List<EquipmentImportPreviewRowViewModel> PreviewRows { get; set; } = new();
    }
}
