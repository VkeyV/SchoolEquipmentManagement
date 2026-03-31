using System.ComponentModel.DataAnnotations;

namespace SchoolEquipmentManagement.Web.ViewModels.Auth
{
    public class VerifyTwoFactorViewModel
    {
        [Required]
        public string ChallengeToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите код из письма.")]
        [Display(Name = "Код подтверждения")]
        public string Code { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
        public string? DestinationHint { get; set; }
    }
}
