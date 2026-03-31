using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Web.Security
{
    public static class SecurityAuditPresentation
    {
        public static string GetEventDisplayName(SecurityAuditEventType eventType)
        {
            return eventType switch
            {
                SecurityAuditEventType.SignInSucceeded => "Успешный вход",
                SecurityAuditEventType.SignInFailed => "Ошибка входа",
                SecurityAuditEventType.SignInLockedOut => "Блокировка входа",
                SecurityAuditEventType.TwoFactorChallengeSent => "Отправлен код 2FA",
                SecurityAuditEventType.TwoFactorSucceeded => "Подтвержден вход 2FA",
                SecurityAuditEventType.TwoFactorFailed => "Ошибка 2FA",
                SecurityAuditEventType.PasswordResetRequested => "Запрос сброса пароля",
                SecurityAuditEventType.PasswordResetCompleted => "Пароль изменен",
                SecurityAuditEventType.Logout => "Выход из системы",
                SecurityAuditEventType.AccessDenied => "Отказ в доступе",
                SecurityAuditEventType.UserCreated => "Создание учетной записи",
                SecurityAuditEventType.UserUpdated => "Изменение учетной записи",
                _ => "Событие безопасности"
            };
        }
    }
}
