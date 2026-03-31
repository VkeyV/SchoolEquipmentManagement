using Microsoft.AspNetCore.Authorization;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;
using SchoolEquipmentManagement.Web.ViewModels.Inventory;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class SecurityAndPresentationTests
    {
        [Fact]
        public void PermissionAuthorizeAttribute_ShouldStorePermission_AndBuildPolicy()
        {
            var attribute = new PermissionAuthorizeAttribute(ModulePermission.ManageUsers);

            Assert.Equal(ModulePermission.ManageUsers, attribute.Permission);
            Assert.Equal("Permission:ManageUsers", attribute.Policy);
            Assert.IsAssignableFrom<AuthorizeAttribute>(attribute);
        }

        [Theory]
        [InlineData(ModulePermission.ViewEquipment, "Permission:ViewEquipment")]
        [InlineData(ModulePermission.ViewSecurityAudit, "Permission:ViewSecurityAudit")]
        public void PermissionPolicyNames_ShouldUseStablePrefix(ModulePermission permission, string expected)
        {
            Assert.Equal(expected, PermissionPolicyNames.For(permission));
        }

        [Fact]
        public void UserPermissionMatrix_ShouldGrantAdministratorAllPermissions()
        {
            foreach (var permission in Enum.GetValues<ModulePermission>())
            {
                Assert.True(UserPermissionMatrix.HasPermission(UserRole.Administrator, permission));
            }
        }

        [Fact]
        public void UserPermissionMatrix_ShouldRestrictTechnicianAdministrativePermissions()
        {
            Assert.True(UserPermissionMatrix.HasPermission(UserRole.Technician, ModulePermission.EditEquipment));
            Assert.True(UserPermissionMatrix.HasPermission(UserRole.Technician, ModulePermission.ManageInventorySession));
            Assert.False(UserPermissionMatrix.HasPermission(UserRole.Technician, ModulePermission.ManageUsers));
            Assert.False(UserPermissionMatrix.HasPermission(UserRole.Technician, ModulePermission.ViewSecurityAudit));
        }

        [Fact]
        public void UserPermissionMatrix_ShouldRestrictResponsibleRole()
        {
            Assert.True(UserPermissionMatrix.HasPermission(UserRole.Responsible, ModulePermission.ViewEquipment));
            Assert.True(UserPermissionMatrix.HasPermission(UserRole.Responsible, ModulePermission.CheckInventory));
            Assert.False(UserPermissionMatrix.HasPermission(UserRole.Responsible, ModulePermission.EditEquipment));
        }

        [Fact]
        public void UserPermissionMatrix_ShouldRestrictViewerRole()
        {
            Assert.True(UserPermissionMatrix.HasPermission(UserRole.Viewer, ModulePermission.ViewEquipment));
            Assert.True(UserPermissionMatrix.HasPermission(UserRole.Viewer, ModulePermission.ViewInventory));
            Assert.False(UserPermissionMatrix.HasPermission(UserRole.Viewer, ModulePermission.CheckInventory));
        }

        [Theory]
        [InlineData(UserRole.Administrator, "Администратор")]
        [InlineData(UserRole.Technician, "Техник")]
        [InlineData(UserRole.Responsible, "Ответственный")]
        [InlineData(UserRole.Viewer, "Наблюдатель")]
        public void UserPermissionMatrix_ShouldReturnRoleDisplayName(UserRole role, string expected)
        {
            Assert.Equal(expected, UserPermissionMatrix.GetRoleDisplayName(role));
        }

        [Theory]
        [InlineData(SecurityAuditEventType.SignInSucceeded, "Успешный вход")]
        [InlineData(SecurityAuditEventType.SignInFailed, "Ошибка входа")]
        [InlineData(SecurityAuditEventType.SignInLockedOut, "Блокировка входа")]
        [InlineData(SecurityAuditEventType.TwoFactorChallengeSent, "Отправлен код 2FA")]
        [InlineData(SecurityAuditEventType.TwoFactorSucceeded, "Подтвержден вход 2FA")]
        [InlineData(SecurityAuditEventType.TwoFactorFailed, "Ошибка 2FA")]
        [InlineData(SecurityAuditEventType.PasswordResetRequested, "Запрос сброса пароля")]
        [InlineData(SecurityAuditEventType.PasswordResetCompleted, "Пароль изменен")]
        [InlineData(SecurityAuditEventType.Logout, "Выход из системы")]
        [InlineData(SecurityAuditEventType.AccessDenied, "Отказ в доступе")]
        [InlineData(SecurityAuditEventType.UserCreated, "Создание учетной записи")]
        [InlineData(SecurityAuditEventType.UserUpdated, "Изменение учетной записи")]
        public void SecurityAuditPresentation_ShouldReturnDisplayName(SecurityAuditEventType eventType, string expected)
        {
            Assert.Equal(expected, SecurityAuditPresentation.GetEventDisplayName(eventType));
        }

        [Fact]
        public void EquipmentDisplayFormatter_ShouldFormatMissingAndExistingValues()
        {
            Assert.Equal("Не указано", EquipmentDisplayFormatter.Text(null));
            Assert.Equal("Ноутбук", EquipmentDisplayFormatter.Text("Ноутбук"));
            Assert.Equal("31.03.2026", EquipmentDisplayFormatter.Date(new DateTime(2026, 3, 31)));
        }

        [Fact]
        public void EquipmentDisplayFormatter_ShouldFormatLocalDateTime()
        {
            var value = new DateTime(2026, 3, 31, 10, 15, 0, DateTimeKind.Local);

            Assert.Equal("31.03.2026 10:15", EquipmentDisplayFormatter.LocalDateTime(value));
        }

        [Theory]
        [InlineData("Списано", "bg-danger text-white")]
        [InlineData("В ремонте", "bg-warning text-dark")]
        [InlineData("Требует диагностики", "bg-warning-subtle text-warning-emphasis")]
        [InlineData("В эксплуатации", "bg-success text-white")]
        [InlineData("На складе", "bg-secondary text-white")]
        [InlineData("В резерве", "bg-info text-dark")]
        [InlineData("Иной статус", "bg-light text-dark border")]
        public void EquipmentStatusPresentation_ShouldReturnBadgeClass(string status, string expected)
        {
            Assert.Equal(expected, EquipmentStatusPresentation.GetBadgeClass(status));
        }

        [Fact]
        public void EquipmentWarrantyPresentation_ShouldReturnNoData_WhenDateMissing()
        {
            Assert.Equal("Нет данных", EquipmentWarrantyPresentation.GetRiskLabel(null));
            Assert.Equal("Дата окончания гарантии не указана.", EquipmentWarrantyPresentation.GetSummary(null));
            Assert.Null(EquipmentWarrantyPresentation.GetDaysLeft(null));
        }

        [Fact]
        public void EquipmentWarrantyPresentation_ShouldReturnCriticalRisk_WhenWarrantyExpired()
        {
            var date = DateTime.Today.AddDays(-1);

            Assert.Equal("Критичный", EquipmentWarrantyPresentation.GetRiskLabel(date));
            Assert.Equal($"Гарантия истекла {date:dd.MM.yyyy}.", EquipmentWarrantyPresentation.GetSummary(date));
        }

        [Fact]
        public void EquipmentWarrantyPresentation_ShouldReturnTodaySummary_WhenWarrantyEndsToday()
        {
            var date = DateTime.Today;

            Assert.Equal("Высокий", EquipmentWarrantyPresentation.GetRiskLabel(date));
            Assert.Equal("Гарантия истекает сегодня.", EquipmentWarrantyPresentation.GetSummary(date));
            Assert.Equal(0, EquipmentWarrantyPresentation.GetDaysLeft(date));
        }

        [Theory]
        [InlineData(15, "Высокий")]
        [InlineData(45, "Средний")]
        [InlineData(75, "Низкий")]
        [InlineData(120, "Плановый")]
        public void EquipmentWarrantyPresentation_ShouldReturnExpectedRiskBand(int daysAhead, string expected)
        {
            var date = DateTime.Today.AddDays(daysAhead);

            Assert.Equal(expected, EquipmentWarrantyPresentation.GetRiskLabel(date));
        }

        [Theory]
        [InlineData("Критичный", "bg-danger text-white")]
        [InlineData("Высокий", "bg-warning text-dark")]
        [InlineData("Средний", "bg-primary text-white")]
        [InlineData("Низкий", "bg-info text-dark")]
        [InlineData("Плановый", "bg-success text-white")]
        [InlineData("Нет данных", "bg-secondary text-white")]
        public void EquipmentWarrantyPresentation_ShouldReturnBadgeClass(string riskLabel, string expected)
        {
            Assert.Equal(expected, EquipmentWarrantyPresentation.GetRiskBadgeClass(riskLabel));
        }

        [Fact]
        public void PaginationDisplayHelper_ShouldReturnZeroRange_WhenListIsEmpty()
        {
            Assert.Equal(0, PaginationDisplayHelper.GetStartItem(1, 20, 0));
            Assert.Equal(0, PaginationDisplayHelper.GetEndItem(1, 20, 0));
        }

        [Fact]
        public void PaginationDisplayHelper_ShouldReturnCorrectRange_WhenListHasItems()
        {
            Assert.Equal(21, PaginationDisplayHelper.GetStartItem(2, 20, 53));
            Assert.Equal(40, PaginationDisplayHelper.GetEndItem(2, 20, 53));
            Assert.Equal(53, PaginationDisplayHelper.GetEndItem(3, 20, 53));
        }

        [Theory]
        [InlineData("Черновик", true, false, false, "bg-secondary text-white")]
        [InlineData("Активна", false, true, false, "bg-primary text-white")]
        [InlineData("Завершена", false, false, true, "bg-success text-white")]
        [InlineData("Отменена", false, false, true, "bg-light text-dark border")]
        public void InventorySessionStatusPresentation_ShouldReturnExpectedFlagsAndBadge(
            string status,
            bool canStart,
            bool canComplete,
            bool isReadOnly,
            string badgeClass)
        {
            Assert.Equal(canStart, InventorySessionStatusPresentation.CanStart(status));
            Assert.Equal(canComplete, InventorySessionStatusPresentation.CanComplete(status));
            Assert.Equal(isReadOnly, InventorySessionStatusPresentation.IsReadOnly(status));
            Assert.Equal(badgeClass, InventorySessionStatusPresentation.GetBadgeClass(status));
        }
    }
}
