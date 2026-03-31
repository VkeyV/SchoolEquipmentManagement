using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentAssignResponsibleViewModel
    {
        public int EquipmentId { get; set; }

        public string InventoryNumber { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? CurrentResponsiblePerson { get; set; }

        [Display(Name = "Новое ответственное лицо")]
        [Required(ErrorMessage = "Укажите ответственное лицо.")]
        [StringLength(128, ErrorMessage = "Ответственное лицо не должно превышать 128 символов.")]
        public string ResponsiblePerson { get; set; } = string.Empty;

        [Display(Name = "Комментарий")]
        [StringLength(256, ErrorMessage = "Комментарий не должен превышать 256 символов.")]
        public string? Comment { get; set; }

        public string? ReturnUrl { get; set; }

        public string DisplayCurrentResponsiblePerson => EquipmentDisplayFormatter.Text(CurrentResponsiblePerson);
    }
}
