using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;
using System.Net.Mail;

namespace SchoolEquipmentManagement.Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHashService _passwordHashService;
        private readonly ISecurityAuditService _securityAuditService;

        public UserManagementService(
            IUserRepository userRepository,
            IPasswordHashService passwordHashService,
            ISecurityAuditService securityAuditService)
        {
            _userRepository = userRepository;
            _passwordHashService = passwordHashService;
            _securityAuditService = securityAuditService;
        }

        public async Task<IReadOnlyList<UserListItemDto>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users
                .Select(user => new UserListItemDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LastSignInAt = user.LastSignInAt,
                    LockoutEndUtc = user.LockoutEndUtc,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                })
                .ToList();
        }

        public async Task<UserEditDto?> GetUserForEditAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
            {
                return null;
            }

            return new UserEditDto
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                TwoFactorEnabled = user.TwoFactorEnabled
            };
        }

        public async Task<int> CreateUserAsync(CreateUserDto dto)
        {
            var userName = NormalizeRequired(dto.UserName, "Укажите логин пользователя.");
            var displayName = NormalizeRequired(dto.DisplayName, "Укажите отображаемое имя пользователя.");
            var email = NormalizeEmail(dto.Email, dto.TwoFactorEnabled);
            var performedByUserName = NormalizeRequired(dto.PerformedByUserName, "Не указан пользователь, выполняющий операцию.");
            ValidatePassword(dto.Password);

            var normalizedUserName = userName.ToUpperInvariant();
            if (await _userRepository.ExistsByNormalizedUserNameAsync(normalizedUserName))
            {
                throw new DomainException("Пользователь с таким логином уже существует.");
            }

            if (!string.IsNullOrWhiteSpace(email) &&
                await _userRepository.ExistsByNormalizedEmailAsync(email.ToUpperInvariant()))
            {
                throw new DomainException("Пользователь с таким адресом электронной почты уже существует.");
            }

            var user = new ApplicationUser(
                userName,
                displayName,
                email,
                _passwordHashService.HashPassword(dto.Password),
                dto.Role,
                dto.IsActive,
                dto.TwoFactorEnabled);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            await _securityAuditService.WriteAsync(new SecurityAuditWriteDto
            {
                EventType = SecurityAuditEventType.UserCreated,
                IsSuccessful = true,
                UserName = performedByUserName,
                TargetUserName = user.UserName,
                Summary = $"Создана учетная запись {user.UserName}."
            });

            return user.Id;
        }

        public async Task UpdateUserAsync(UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.Id)
                ?? throw new DomainException("Пользователь не найден.");

            var originalDisplayName = user.DisplayName;
            var originalEmail = user.Email;
            var originalRole = user.Role;
            var originalIsActive = user.IsActive;
            var originalTwoFactorEnabled = user.TwoFactorEnabled;

            var displayName = NormalizeRequired(dto.DisplayName, "Укажите отображаемое имя пользователя.");
            var email = NormalizeEmail(dto.Email, dto.TwoFactorEnabled);
            var performedByUserName = NormalizeRequired(dto.PerformedByUserName, "Не указан пользователь, выполняющий операцию.");
            var wasActiveAdministrator = user.IsActive && user.Role == UserRole.Administrator;
            var remainsActiveAdministrator = dto.IsActive && dto.Role == UserRole.Administrator;

            if (!string.IsNullOrWhiteSpace(email) &&
                !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase) &&
                await _userRepository.ExistsByNormalizedEmailAsync(email.ToUpperInvariant()))
            {
                throw new DomainException("Пользователь с таким адресом электронной почты уже существует.");
            }

            if (string.Equals(user.UserName, performedByUserName, StringComparison.OrdinalIgnoreCase) &&
                !remainsActiveAdministrator)
            {
                throw new DomainException("Нельзя отключить собственную учетную запись или снять с себя роль администратора.");
            }

            if (wasActiveAdministrator && !remainsActiveAdministrator)
            {
                var activeAdministrators = await _userRepository.CountActiveAdministratorsAsync();
                if (activeAdministrators <= 1)
                {
                    throw new DomainException("В системе должен оставаться хотя бы один активный администратор.");
                }
            }

            user.UpdateProfile(displayName, dto.Role);
            user.UpdateSecuritySettings(email, dto.TwoFactorEnabled);

            if (dto.IsActive)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                ValidatePassword(dto.NewPassword);
                user.UpdatePasswordHash(_passwordHashService.HashPassword(dto.NewPassword));
            }

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            await _securityAuditService.WriteAsync(new SecurityAuditWriteDto
            {
                EventType = SecurityAuditEventType.UserUpdated,
                IsSuccessful = true,
                UserName = performedByUserName,
                TargetUserName = user.UserName,
                Summary = BuildUpdateSummary(
                    user.UserName,
                    originalDisplayName,
                    displayName,
                    originalEmail,
                    email,
                    originalRole,
                    dto.Role,
                    originalIsActive,
                    dto.IsActive,
                    originalTwoFactorEnabled,
                    dto.TwoFactorEnabled,
                    !string.IsNullOrWhiteSpace(dto.NewPassword))
            });
        }

        private static string BuildUpdateSummary(
            string userName,
            string originalDisplayName,
            string displayName,
            string? originalEmail,
            string? email,
            UserRole originalRole,
            UserRole role,
            bool originalIsActive,
            bool isActive,
            bool originalTwoFactorEnabled,
            bool twoFactorEnabled,
            bool passwordChanged)
        {
            var changes = new List<string>();

            if (!string.Equals(originalDisplayName, displayName, StringComparison.Ordinal))
            {
                changes.Add("изменено имя");
            }

            if (!string.Equals(originalEmail, email, StringComparison.OrdinalIgnoreCase))
            {
                changes.Add("обновлена почта");
            }

            if (originalRole != role)
            {
                changes.Add("изменена роль");
            }

            if (originalIsActive != isActive)
            {
                changes.Add(isActive ? "учетная запись активирована" : "учетная запись отключена");
            }

            if (originalTwoFactorEnabled != twoFactorEnabled)
            {
                changes.Add(twoFactorEnabled ? "включена 2FA" : "выключена 2FA");
            }

            if (passwordChanged)
            {
                changes.Add("сменен пароль");
            }

            if (changes.Count == 0)
            {
                return $"Подтверждены настройки учетной записи {userName}.";
            }

            return $"Обновлена учетная запись {userName}: {string.Join(", ", changes)}.";
        }

        private static string NormalizeRequired(string? value, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException(errorMessage);
            }

            return value.Trim();
        }

        private static string? NormalizeEmail(string? email, bool twoFactorEnabled)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                if (twoFactorEnabled)
                {
                    throw new DomainException("Для включения двухфакторной аутентификации укажите адрес электронной почты.");
                }

                return null;
            }

            try
            {
                return new MailAddress(email.Trim()).Address;
            }
            catch (FormatException)
            {
                throw new DomainException("Укажите корректный адрес электронной почты.");
            }
        }

        private static void ValidatePassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new DomainException("Укажите пароль.");
            }

            if (password.Trim().Length < 8)
            {
                throw new DomainException("Пароль должен содержать не менее 8 символов.");
            }
        }
    }
}
