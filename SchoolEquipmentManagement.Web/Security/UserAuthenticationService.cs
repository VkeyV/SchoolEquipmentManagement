using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Infrastructure.Data;
using SchoolEquipmentManagement.Infrastructure.Security;
using System.Security.Cryptography;

namespace SchoolEquipmentManagement.Web.Security
{
    public sealed class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailSender _emailSender;
        private readonly ISecurityAuditService _securityAuditService;
        private readonly SecurityOptions _securityOptions;

        public UserAuthenticationService(
            ApplicationDbContext dbContext,
            IEmailSender emailSender,
            ISecurityAuditService securityAuditService,
            IOptions<SecurityOptions> securityOptions)
        {
            _dbContext = dbContext;
            _emailSender = emailSender;
            _securityAuditService = securityAuditService;
            _securityOptions = securityOptions.Value;
        }

        public async Task<SignInPreparationResult> BeginSignInAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return new SignInPreparationResult(SignInPreparationStatus.InvalidCredentials);
            }

            var normalizedUserName = userName.Trim().ToUpperInvariant();
            var attemptedUserName = userName.Trim();
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName && x.IsActive, cancellationToken);

            if (user is null)
            {
                await WriteAuditAsync(
                    SecurityAuditEventType.SignInFailed,
                    false,
                    "Неудачная попытка входа: неверный логин или пароль.",
                    attemptedUserName,
                    cancellationToken);

                return new SignInPreparationResult(SignInPreparationStatus.InvalidCredentials);
            }

            var utcNow = DateTime.UtcNow;
            if (user.IsLockedOut(utcNow))
            {
                await WriteAuditAsync(
                    SecurityAuditEventType.SignInLockedOut,
                    false,
                    $"Вход заблокирован до {user.LockoutEndUtc!.Value.ToLocalTime():dd.MM.yyyy HH:mm}.",
                    user.UserName,
                    cancellationToken);

                return new SignInPreparationResult(
                    SignInPreparationStatus.LockedOut,
                    ErrorMessage: $"Учетная запись временно заблокирована до {user.LockoutEndUtc.Value.ToLocalTime():dd.MM.yyyy HH:mm}.");
            }

            if (!PasswordHashUtility.VerifyPassword(password, user.PasswordHash))
            {
                var lockoutApplied = user.RegisterFailedSignInAttempt(
                    GetMaxFailedSignInAttempts(),
                    TimeSpan.FromMinutes(GetLockoutMinutes()),
                    utcNow);

                await _dbContext.SaveChangesAsync(cancellationToken);

                if (lockoutApplied)
                {
                    await WriteAuditAsync(
                        SecurityAuditEventType.SignInLockedOut,
                        false,
                        $"Учетная запись заблокирована до {user.LockoutEndUtc!.Value.ToLocalTime():dd.MM.yyyy HH:mm} после серии неудачных входов.",
                        user.UserName,
                        cancellationToken);

                    return new SignInPreparationResult(
                        SignInPreparationStatus.LockedOut,
                        ErrorMessage: $"Учетная запись временно заблокирована до {user.LockoutEndUtc.Value.ToLocalTime():dd.MM.yyyy HH:mm}.");
                }

                await WriteAuditAsync(
                    SecurityAuditEventType.SignInFailed,
                    false,
                    "Неудачная попытка входа: неверный логин или пароль.",
                    user.UserName,
                    cancellationToken);

                return new SignInPreparationResult(SignInPreparationStatus.InvalidCredentials);
            }

            if (!user.TwoFactorEnabled)
            {
                user.RecordSuccessfulSignIn(utcNow);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await WriteAuditAsync(
                    SecurityAuditEventType.SignInSucceeded,
                    true,
                    "Пользователь успешно вошел в систему.",
                    user.UserName,
                    cancellationToken);

                return new SignInPreparationResult(
                    SignInPreparationStatus.SignedIn,
                    ToAuthenticatedUser(user));
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                await WriteAuditAsync(
                    SecurityAuditEventType.SignInFailed,
                    false,
                    "Невозможно завершить вход: для пользователя не настроена почта для 2FA.",
                    user.UserName,
                    cancellationToken);

                return new SignInPreparationResult(
                    SignInPreparationStatus.TwoFactorUnavailable,
                    ErrorMessage: "Для этой учетной записи не настроена электронная почта для 2FA. Обратитесь к администратору.");
            }

            user.ClearLockout();
            await _dbContext.SaveChangesAsync(cancellationToken);

            var challenge = await CreateSecurityCodeAsync(
                user,
                UserSecurityCodePurpose.TwoFactorSignIn,
                TimeSpan.FromMinutes(GetTwoFactorCodeLifetimeMinutes()),
                cancellationToken);

            await _emailSender.SendAsync(
                new EmailMessage(
                    user.Email,
                    "Код подтверждения входа",
                    $"Код для входа в систему: {challenge.Code}\n\nКод действует {GetTwoFactorCodeLifetimeMinutes()} минут."),
                cancellationToken);

            await WriteAuditAsync(
                SecurityAuditEventType.TwoFactorChallengeSent,
                true,
                "На электронную почту отправлен код подтверждения входа.",
                user.UserName,
                cancellationToken);

            return new SignInPreparationResult(
                SignInPreparationStatus.RequiresTwoFactor,
                ChallengeToken: challenge.ChallengeToken,
                DestinationHint: MaskEmail(user.Email));
        }

        public async Task<CodeVerificationResult> VerifyTwoFactorAsync(string challengeToken, string code, CancellationToken cancellationToken = default)
        {
            var challenge = await FindActiveCodeAsync(challengeToken, UserSecurityCodePurpose.TwoFactorSignIn, cancellationToken);
            if (challenge is null || string.IsNullOrWhiteSpace(code) || !PasswordHashUtility.VerifyPassword(code.Trim(), challenge.CodeHash))
            {
                await WriteAuditAsync(
                    SecurityAuditEventType.TwoFactorFailed,
                    false,
                    "Ошибка подтверждения двухфакторного входа.",
                    challenge?.User.UserName,
                    cancellationToken);

                return new CodeVerificationResult(false, ErrorMessage: "Неверный или просроченный код подтверждения.");
            }

            challenge.User.RecordSuccessfulSignIn(DateTime.UtcNow);
            challenge.Consume();
            await _dbContext.SaveChangesAsync(cancellationToken);

            await WriteAuditAsync(
                SecurityAuditEventType.TwoFactorSucceeded,
                true,
                "Пользователь успешно подтвердил вход кодом 2FA.",
                challenge.User.UserName,
                cancellationToken);

            return new CodeVerificationResult(true, ToAuthenticatedUser(challenge.User));
        }

        public async Task RequestPasswordResetAsync(string loginOrEmail, string resetPageUrlTemplate, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(loginOrEmail))
            {
                return;
            }

            var normalized = loginOrEmail.Trim().ToUpperInvariant();
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(
                    x => x.IsActive &&
                         (x.NormalizedUserName == normalized || x.NormalizedEmail == normalized),
                    cancellationToken);

            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                await WriteAuditAsync(
                    SecurityAuditEventType.PasswordResetRequested,
                    false,
                    "Запрос на восстановление доступа отклонен.",
                    loginOrEmail.Trim(),
                    cancellationToken);

                return;
            }

            var challenge = await CreateSecurityCodeAsync(
                user,
                UserSecurityCodePurpose.PasswordReset,
                TimeSpan.FromMinutes(GetPasswordResetCodeLifetimeMinutes()),
                cancellationToken);
            var resetUrl = resetPageUrlTemplate.Replace("{token}", Uri.EscapeDataString(challenge.ChallengeToken), StringComparison.Ordinal);

            await _emailSender.SendAsync(
                new EmailMessage(
                    user.Email,
                    "Восстановление доступа",
                    $"Для восстановления доступа перейдите по ссылке:\n{resetUrl}\n\nКод подтверждения: {challenge.Code}\nКод действует {GetPasswordResetCodeLifetimeMinutes()} минут."),
                cancellationToken);

            await WriteAuditAsync(
                SecurityAuditEventType.PasswordResetRequested,
                true,
                "Сформирован запрос на восстановление доступа.",
                user.UserName,
                cancellationToken);
        }

        public async Task<CodeVerificationResult> ResetPasswordAsync(string challengeToken, string code, string newPassword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Trim().Length < 8)
            {
                return new CodeVerificationResult(false, ErrorMessage: "Пароль должен содержать не менее 8 символов.");
            }

            var challenge = await FindActiveCodeAsync(challengeToken, UserSecurityCodePurpose.PasswordReset, cancellationToken);
            if (challenge is null || string.IsNullOrWhiteSpace(code) || !PasswordHashUtility.VerifyPassword(code.Trim(), challenge.CodeHash))
            {
                await WriteAuditAsync(
                    SecurityAuditEventType.PasswordResetCompleted,
                    false,
                    "Не удалось завершить сброс пароля: неверный или просроченный код.",
                    challenge?.User.UserName,
                    cancellationToken);

                return new CodeVerificationResult(false, ErrorMessage: "Неверный или просроченный код подтверждения.");
            }

            challenge.User.UpdatePasswordHash(PasswordHashUtility.HashPassword(newPassword.Trim()));
            challenge.User.ClearLockout();
            challenge.Consume();

            var otherResetCodes = await _dbContext.UserSecurityCodes
                .Where(x => x.UserId == challenge.UserId &&
                            x.Purpose == UserSecurityCodePurpose.PasswordReset &&
                            x.ConsumedAt == null &&
                            x.Id != challenge.Id)
                .ToListAsync(cancellationToken);

            foreach (var item in otherResetCodes)
            {
                item.Consume();
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await WriteAuditAsync(
                SecurityAuditEventType.PasswordResetCompleted,
                true,
                "Пароль успешно изменен через восстановление доступа.",
                challenge.User.UserName,
                cancellationToken);

            return new CodeVerificationResult(true, ToAuthenticatedUser(challenge.User));
        }

        private async Task<(string ChallengeToken, string Code)> CreateSecurityCodeAsync(
            ApplicationUser user,
            UserSecurityCodePurpose purpose,
            TimeSpan lifetime,
            CancellationToken cancellationToken)
        {
            var existingCodes = await _dbContext.UserSecurityCodes
                .Where(x => x.UserId == user.Id && x.Purpose == purpose && x.ConsumedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var item in existingCodes)
            {
                item.Consume();
            }

            var challengeToken = Guid.NewGuid().ToString("N");
            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var entity = new UserSecurityCode(
                user.Id,
                purpose,
                challengeToken,
                PasswordHashUtility.HashPassword(code),
                DateTime.UtcNow.Add(lifetime));

            await _dbContext.UserSecurityCodes.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (challengeToken, code);
        }

        private async Task<UserSecurityCode?> FindActiveCodeAsync(string challengeToken, UserSecurityCodePurpose purpose, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(challengeToken))
            {
                return null;
            }

            var challenge = await _dbContext.UserSecurityCodes
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.ChallengeToken == challengeToken && x.Purpose == purpose, cancellationToken);

            if (challenge is null || !challenge.IsActive(DateTime.UtcNow) || !challenge.User.IsActive)
            {
                return null;
            }

            return challenge;
        }

        private static AuthenticatedUser ToAuthenticatedUser(ApplicationUser user)
        {
            return new AuthenticatedUser(user.Id, user.UserName, user.DisplayName, user.Role);
        }

        private static string MaskEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            if (atIndex <= 1)
            {
                return email;
            }

            var localPart = email[..atIndex];
            var domain = email[atIndex..];
            return $"{localPart[0]}***{localPart[^1]}{domain}";
        }

        private int GetMaxFailedSignInAttempts() => Math.Max(1, _securityOptions.MaxFailedSignInAttempts);

        private int GetLockoutMinutes() => Math.Max(1, _securityOptions.LockoutMinutes);

        private int GetTwoFactorCodeLifetimeMinutes() => Math.Max(1, _securityOptions.TwoFactorCodeLifetimeMinutes);

        private int GetPasswordResetCodeLifetimeMinutes() => Math.Max(1, _securityOptions.PasswordResetCodeLifetimeMinutes);

        private Task WriteAuditAsync(
            SecurityAuditEventType eventType,
            bool isSuccessful,
            string summary,
            string? userName,
            CancellationToken cancellationToken)
        {
            return _securityAuditService.WriteAsync(new SecurityAuditWriteDto
            {
                EventType = eventType,
                IsSuccessful = isSuccessful,
                Summary = summary,
                UserName = userName
            }, cancellationToken);
        }
    }
}
