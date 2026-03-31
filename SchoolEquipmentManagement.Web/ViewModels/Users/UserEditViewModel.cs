using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolEquipmentManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Users
{
    public class UserEditViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Логин")]
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
        public bool IsActive { get; set; }

        [Display(Name = "Включить 2FA по почте")]
        public bool TwoFactorEnabled { get; set; }

        [MinLength(8, ErrorMessage = "Пароль должен содержать не менее 8 символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        public string? ConfirmNewPassword { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hasNewPassword = !string.IsNullOrWhiteSpace(NewPassword);
            var hasConfirmation = !string.IsNullOrWhiteSpace(ConfirmNewPassword);

            if (hasNewPassword != hasConfirmation)
            {
                yield return new ValidationResult(
                    "Для смены пароля заполните оба поля.",
                    new[] { nameof(NewPassword), nameof(ConfirmNewPassword) });
            }

            if (hasNewPassword &&
                !string.Equals(NewPassword, ConfirmNewPassword, StringComparison.Ordinal))
            {
                yield return new ValidationResult(
                    "Пароли не совпадают.",
                    new[] { nameof(ConfirmNewPassword) });
            }

            if (TwoFactorEnabled && string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(
                    "Для включения 2FA укажите адрес электронной почты.",
                    new[] { nameof(Email), nameof(TwoFactorEnabled) });
            }
        }
    }
}
