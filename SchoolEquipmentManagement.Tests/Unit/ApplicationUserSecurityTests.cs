using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class ApplicationUserSecurityTests
    {
        [Fact]
        public void RegisterFailedSignInAttempt_ShouldLockUser_WhenLimitReached()
        {
            var user = new ApplicationUser(
                "viewer",
                "Наблюдатель",
                "viewer@example.local",
                "hashed",
                UserRole.Viewer);

            var utcNow = new DateTime(2026, 3, 30, 12, 0, 0, DateTimeKind.Utc);

            Assert.False(user.RegisterFailedSignInAttempt(3, TimeSpan.FromMinutes(15), utcNow));
            Assert.False(user.RegisterFailedSignInAttempt(3, TimeSpan.FromMinutes(15), utcNow.AddMinutes(1)));

            var locked = user.RegisterFailedSignInAttempt(3, TimeSpan.FromMinutes(15), utcNow.AddMinutes(2));

            Assert.True(locked);
            Assert.True(user.IsLockedOut(utcNow.AddMinutes(3)));
            Assert.Equal(0, user.FailedSignInAttempts);
            Assert.Equal(utcNow.AddMinutes(17), user.LockoutEndUtc);
        }

        [Fact]
        public void RecordSuccessfulSignIn_ShouldResetFailuresAndSetLastSignInAt()
        {
            var user = new ApplicationUser(
                "tech",
                "Техник",
                "tech@example.local",
                "hashed",
                UserRole.Technician);

            var utcNow = new DateTime(2026, 3, 30, 15, 30, 0, DateTimeKind.Utc);
            user.RegisterFailedSignInAttempt(5, TimeSpan.FromMinutes(15), utcNow);
            user.RecordSuccessfulSignIn(utcNow.AddMinutes(1));

            Assert.Equal(0, user.FailedSignInAttempts);
            Assert.Null(user.LockoutEndUtc);
            Assert.Equal(utcNow.AddMinutes(1), user.LastSignInAt);
        }
    }
}
