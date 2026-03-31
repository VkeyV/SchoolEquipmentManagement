using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentChangeLocationViewModel
    {
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CurrentLocationId { get; set; }
        public string CurrentLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите новое местоположение.")]
        [Display(Name = "Новое местоположение")]
        public int? NewLocationId { get; set; }

        [Display(Name = "Комментарий")]
        [StringLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов.")]
        public string? Comment { get; set; }

        public List<SelectListItem> AvailableLocations { get; set; } = new();
    }
}
