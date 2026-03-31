using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentChangeStatusViewModel
    {
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CurrentStatusId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите новый статус.")]
        [Display(Name = "Новый статус")]
        public int? NewStatusId { get; set; }

        [Display(Name = "Комментарий")]
        [StringLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов.")]
        public string? Comment { get; set; }

        public List<SelectListItem> AvailableStatuses { get; set; } = new();
    }
}
