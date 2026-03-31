using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Auth
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string ChallengeToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите код из письма.")]
        [Display(Name = "Код подтверждения")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите новый пароль.")]
        [MinLength(8, ErrorMessage = "Пароль должен содержать не менее 8 символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтвердите новый пароль.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают.")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
