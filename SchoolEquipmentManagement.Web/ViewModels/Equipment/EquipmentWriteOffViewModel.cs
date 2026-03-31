using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentWriteOffViewModel
    {
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите причину списания.")]
        [Display(Name = "Причина списания")]
        [StringLength(500, ErrorMessage = "Причина списания не должна превышать 500 символов.")]
        public string Comment { get; set; } = string.Empty;
    }
}
