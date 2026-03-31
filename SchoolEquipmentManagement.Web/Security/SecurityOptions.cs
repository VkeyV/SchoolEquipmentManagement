namespace SchoolEquipmentManagement.Web.Security
{
    public sealed class SecurityOptions
    {
        public const string SectionName = "Security";

        public int MaxFailedSignInAttempts { get; set; } = 5;
        public int LockoutMinutes { get; set; } = 15;
        public int TwoFactorCodeLifetimeMinutes { get; set; } = 10;
        public int PasswordResetCodeLifetimeMinutes { get; set; } = 15;
        public int AuditPageSize { get; set; } = 100;
    }
}
