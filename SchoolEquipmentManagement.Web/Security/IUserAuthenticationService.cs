namespace SchoolEquipmentManagement.Web.Security
{
    public interface IUserAuthenticationService
    {
        Task<SignInPreparationResult> BeginSignInAsync(string userName, string password, CancellationToken cancellationToken = default);
        Task<CodeVerificationResult> VerifyTwoFactorAsync(string challengeToken, string code, CancellationToken cancellationToken = default);
        Task RequestPasswordResetAsync(string loginOrEmail, string resetPageUrlTemplate, CancellationToken cancellationToken = default);
        Task<CodeVerificationResult> ResetPasswordAsync(string challengeToken, string code, string newPassword, CancellationToken cancellationToken = default);
    }
}
