namespace SchoolEquipmentManagement.Domain.Enums
{
    public enum SecurityAuditEventType
    {
        SignInSucceeded,
        SignInFailed,
        SignInLockedOut,
        TwoFactorChallengeSent,
        TwoFactorSucceeded,
        TwoFactorFailed,
        PasswordResetRequested,
        PasswordResetCompleted,
        Logout,
        AccessDenied,
        UserCreated,
        UserUpdated
    }
}
