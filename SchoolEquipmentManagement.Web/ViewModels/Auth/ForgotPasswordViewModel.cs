using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Укажите логин или адрес электронной почты.")]
        [Display(Name = "Логин или электронная почта")]
        public string LoginOrEmail { get; set; } = string.Empty;
    }
}
