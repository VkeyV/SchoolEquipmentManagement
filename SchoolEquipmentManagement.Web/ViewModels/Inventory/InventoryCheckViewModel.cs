using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Inventory
{
    public class InventoryCheckViewModel
    {
        public int SessionId { get; set; }
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ExpectedLocationName { get; set; } = string.Empty;
        public string EquipmentStatusName { get; set; } = string.Empty;

        [Display(Name = "Оборудование найдено")]
        public bool IsFound { get; set; } = true;

        [Display(Name = "Фактическое местоположение")]
        public int? ActualLocationId { get; set; }

        [Display(Name = "Комментарий")]
        [StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов.")]
        public string? ConditionComment { get; set; }

        public List<SelectListItem> Locations { get; set; } = new();
    }
}
