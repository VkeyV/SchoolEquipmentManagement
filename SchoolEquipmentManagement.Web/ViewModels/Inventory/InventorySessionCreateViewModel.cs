using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Inventory
{
    public class InventorySessionCreateViewModel
    {
        [Required(ErrorMessage = "Укажите название сессии инвентаризации.")]
        [Display(Name = "Название сессии")]
        [StringLength(200, ErrorMessage = "Название сессии не должно превышать 200 символов.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Дата начала")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;
    }
}
