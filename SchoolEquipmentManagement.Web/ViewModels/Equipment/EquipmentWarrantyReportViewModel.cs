using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentWarrantyReportViewModel
    {
        public string? WarrantyFilter { get; set; }
        public List<SelectListItem> WarrantyFilters { get; set; } = new();
        public List<EquipmentWarrantyReportItemViewModel> Items { get; set; } = new();

        public int TotalCount => Items.Count;
    }
}
