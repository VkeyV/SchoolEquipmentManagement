using SchoolEquipmentManagement.Domain.Common;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Domain.Entities
{
    public class ApplicationUser : AuditableEntity
    {
        public string UserName { get; private set; }
        public string NormalizedUserName { get; private set; }
        public string DisplayName { get; private set; }
        public string? Email { get; private set; }
        public string? NormalizedEmail { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }
        public bool TwoFactorEnabled { get; private set; }
        public int FailedSignInAttempts { get; private set; }
        public DateTime? LockoutEndUtc { get; private set; }
        public DateTime? LastSignInAt { get; private set; }

        private ApplicationUser()
        {
            UserName = null!;
            NormalizedUserName = null!;
            DisplayName = null!;
            PasswordHash = null!;
        }

        public ApplicationUser(
            string userName,
            string displayName,
            string? email,
            string passwordHash,
            UserRole role,
            bool isActive = true,
            bool twoFactorEnabled = false)
        {
            SetUserName(userName);
            SetDisplayName(displayName);
            SetEmail(email);
            SetPasswordHash(passwordHash);

            Role = role;
            IsActive = isActive;
            TwoFactorEnabled = twoFactorEnabled;
        }

        public void UpdatePasswordHash(string passwordHash)
        {
            SetPasswordHash(passwordHash);
            MarkAsUpdated();
        }

        public void UpdateProfile(string displayName, UserRole role)
        {
            SetDisplayName(displayName);
            Role = role;
            MarkAsUpdated();
        }

        public void UpdateSecuritySettings(string? email, bool twoFactorEnabled)
        {
            SetEmail(email);

            if (twoFactorEnabled && string.IsNullOrWhiteSpace(Email))
            {
                throw new DomainException("Для включения двухфакторной аутентификации укажите адрес электронной почты.");
            }

            TwoFactorEnabled = twoFactorEnabled;
            MarkAsUpdated();
        }

        public void Activate()
        {
            IsActive = true;
            MarkAsUpdated();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsUpdated();
        }

        public bool IsLockedOut(DateTime utcNow)
        {
            return LockoutEndUtc.HasValue && LockoutEndUtc.Value > utcNow;
        }

        public bool RegisterFailedSignInAttempt(int maxFailedAttempts, TimeSpan lockoutDuration, DateTime utcNow)
        {
            if (maxFailedAttempts <= 0)
            {
                throw new DomainException("Некорректно настроено количество попыток входа.");
            }

            if (lockoutDuration <= TimeSpan.Zero)
            {
                throw new DomainException("Некорректно настроена длительность блокировки.");
            }

            FailedSignInAttempts++;

            if (FailedSignInAttempts < maxFailedAttempts)
            {
                MarkAsUpdated();
                return false;
            }

            FailedSignInAttempts = 0;
            LockoutEndUtc = utcNow.Add(lockoutDuration);
            MarkAsUpdated();
            return true;
        }

        public void RecordSuccessfulSignIn(DateTime utcNow)
        {
            FailedSignInAttempts = 0;
            LockoutEndUtc = null;
            LastSignInAt = utcNow;
            MarkAsUpdated();
        }

        public void ClearLockout()
        {
            FailedSignInAttempts = 0;
            LockoutEndUtc = null;
            MarkAsUpdated();
        }

        private void SetUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new DomainException("Логин пользователя не может быть пустым.");
            }

            UserName = userName.Trim();
            NormalizedUserName = UserName.ToUpperInvariant();
        }

        private void SetDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new DomainException("Отображаемое имя пользователя не может быть пустым.");
            }

            DisplayName = displayName.Trim();
        }

        private void SetEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Email = null;
                NormalizedEmail = null;
                return;
            }

            var normalizedEmail = email.Trim();
            if (!normalizedEmail.Contains('@'))
            {
                throw new DomainException("Укажите корректный адрес электронной почты.");
            }

            Email = normalizedEmail;
            NormalizedEmail = normalizedEmail.ToUpperInvariant();
        }

        private void SetPasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new DomainException("Хэш пароля не может быть пустым.");
            }

            PasswordHash = passwordHash.Trim();
        }
    }
}
