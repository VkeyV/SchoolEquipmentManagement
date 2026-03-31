using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolEquipmentManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Users
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Укажите логин.")]
        [Display(Name = "Логин")]
        [StringLength(64, ErrorMessage = "Логин не должен превышать 64 символа.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите отображаемое имя.")]
        [Display(Name = "Отображаемое имя")]
        [StringLength(128, ErrorMessage = "Отображаемое имя не должно превышать 128 символов.")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "Электронная почта")]
        [EmailAddress(ErrorMessage = "Укажите корректный адрес электронной почты.")]
        [StringLength(256, ErrorMessage = "Адрес электронной почты не должен превышать 256 символов.")]
        public string? Email { get; set; }

        [Display(Name = "Роль")]
        [Required(ErrorMessage = "Выберите роль.")]
        public UserRole? Role { get; set; }

        [Display(Name = "Активна")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Включить 2FA по почте")]
        public bool TwoFactorEnabled { get; set; }

        [Required(ErrorMessage = "Укажите пароль.")]
        [MinLength(8, ErrorMessage = "Пароль должен содержать не менее 8 символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтвердите пароль.")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public List<SelectListItem> Roles { get; set; } = new();
    }
}
