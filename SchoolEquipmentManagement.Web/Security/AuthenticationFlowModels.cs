namespace SchoolEquipmentManagement.Web.Security
{
    public enum SignInPreparationStatus
    {
        InvalidCredentials,
        LockedOut,
        SignedIn,
        RequiresTwoFactor,
        TwoFactorUnavailable
    }

    public sealed record SignInPreparationResult(
        SignInPreparationStatus Status,
        AuthenticatedUser? User = null,
        string? ChallengeToken = null,
        string? DestinationHint = null,
        string? ErrorMessage = null);

    public sealed record CodeVerificationResult(
        bool Succeeded,
        AuthenticatedUser? User = null,
        string? ErrorMessage = null);
}
