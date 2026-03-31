using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentCreateViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Укажите инвентарный номер.")]
        [Display(Name = "Инвентарный номер")]
        [StringLength(50, ErrorMessage = "Инвентарный номер не должен превышать 50 символов.")]
        public string InventoryNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите наименование оборудования.")]
        [Display(Name = "Наименование")]
        [StringLength(200, ErrorMessage = "Наименование не должно превышать 200 символов.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите тип оборудования.")]
        [Display(Name = "Тип оборудования")]
        public int? EquipmentTypeId { get; set; }

        [Required(ErrorMessage = "Выберите статус.")]
        [Display(Name = "Статус")]
        public int? EquipmentStatusId { get; set; }

        [Required(ErrorMessage = "Выберите местоположение.")]
        [Display(Name = "Местоположение")]
        public int? LocationId { get; set; }

        [Display(Name = "Серийный номер")]
        [StringLength(100, ErrorMessage = "Серийный номер не должен превышать 100 символов.")]
        public string? SerialNumber { get; set; }

        [Display(Name = "Модель")]
        [StringLength(150, ErrorMessage = "Модель не должна превышать 150 символов.")]
        public string? Model { get; set; }

        [Display(Name = "Производитель")]
        [StringLength(150, ErrorMessage = "Производитель не должен превышать 150 символов.")]
        public string? Manufacturer { get; set; }

        [Display(Name = "Дата покупки")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = "Дата ввода в эксплуатацию")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CommissioningDate { get; set; }

        [Display(Name = "Дата окончания гарантии")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? WarrantyEndDate { get; set; }

        [Display(Name = "Ответственное лицо")]
        [StringLength(150, ErrorMessage = "Ответственное лицо не должно превышать 150 символов.")]
        public string? ResponsiblePerson { get; set; }

        [Display(Name = "Примечание")]
        [StringLength(1000, ErrorMessage = "Примечание не должно превышать 1000 символов.")]
        public string? Notes { get; set; }

        [Display(Name = "Фотография оборудования")]
        public IFormFile? Photo { get; set; }

        public string? ExistingPhotoUrl { get; set; }
        public bool RemovePhoto { get; set; }

        public List<SelectListItem> EquipmentTypes { get; set; } = new();
        public List<SelectListItem> EquipmentStatuses { get; set; } = new();
        public List<SelectListItem> Locations { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PurchaseDate.HasValue && CommissioningDate.HasValue &&
                CommissioningDate.Value.Date < PurchaseDate.Value.Date)
            {
                yield return new ValidationResult(
                    "Дата ввода в эксплуатацию не может быть раньше даты покупки.",
                    new[] { nameof(CommissioningDate) });
            }

            if (CommissioningDate.HasValue && WarrantyEndDate.HasValue &&
                WarrantyEndDate.Value.Date < CommissioningDate.Value.Date)
            {
                yield return new ValidationResult(
                    "Дата окончания гарантии не может быть раньше даты ввода в эксплуатацию.",
                    new[] { nameof(WarrantyEndDate) });
            }
        }
    }
}
