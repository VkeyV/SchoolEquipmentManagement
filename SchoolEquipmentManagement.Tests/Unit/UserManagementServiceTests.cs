using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Services;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;
using SchoolEquipmentManagement.Tests.TestSupport;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class UserManagementServiceTests
    {
        [Fact]
        public async Task CreateUserAsync_ShouldPersistUserWithHashedPassword_AndWriteAuditEntry()
        {
            var repository = new FakeUserRepository();
            var auditService = new FakeSecurityAuditService();
            var service = new UserManagementService(repository, new FakePasswordHashService(), auditService);

            var userId = await service.CreateUserAsync(new CreateUserDto
            {
                UserName = "operator",
                DisplayName = "Оператор",
                Email = "operator@example.local",
                Password = "StrongPass1!",
                Role = UserRole.Technician,
                IsActive = true,
                TwoFactorEnabled = true,
                PerformedByUserName = "admin"
            });

            var createdUser = Assert.Single(await repository.GetAllAsync());
            Assert.Equal(userId, createdUser.Id);
            Assert.Equal("operator", createdUser.UserName);
            Assert.Equal("Оператор", createdUser.DisplayName);
            Assert.Equal("operator@example.local", createdUser.Email);
            Assert.Equal("hashed:StrongPass1!", createdUser.PasswordHash);
            Assert.Equal(UserRole.Technician, createdUser.Role);
            Assert.True(createdUser.IsActive);
            Assert.True(createdUser.TwoFactorEnabled);

            var auditEntry = Assert.Single(auditService.Entries);
            Assert.Equal(SecurityAuditEventType.UserCreated, auditEntry.EventType);
            Assert.True(auditEntry.IsSuccessful);
            Assert.Equal("admin", auditEntry.UserName);
            Assert.Equal("operator", auditEntry.TargetUserName);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrow_WhenTryingToDeactivateLastActiveAdministrator()
        {
            var repository = new FakeUserRepository();
            repository.Seed(TestEntityFactory.CreateUser(id: 1, userName: "admin", role: UserRole.Administrator, isActive: true));

            var service = new UserManagementService(repository, new FakePasswordHashService(), new FakeSecurityAuditService());

            var action = () => service.UpdateUserAsync(new UpdateUserDto
            {
                Id = 1,
                DisplayName = "Администратор",
                Role = UserRole.Viewer,
                IsActive = false,
                PerformedByUserName = "other-admin"
            });

            var exception = await Assert.ThrowsAsync<DomainException>(action);
            Assert.Contains("активный администратор", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrow_WhenUserDemotesOwnAdministratorAccount()
        {
            var repository = new FakeUserRepository();
            repository.Seed(
                TestEntityFactory.CreateUser(id: 1, userName: "admin", role: UserRole.Administrator, isActive: true),
                TestEntityFactory.CreateUser(id: 2, userName: "backup-admin", role: UserRole.Administrator, isActive: true));

            var service = new UserManagementService(repository, new FakePasswordHashService(), new FakeSecurityAuditService());

            var action = () => service.UpdateUserAsync(new UpdateUserDto
            {
                Id = 1,
                DisplayName = "Администратор",
                Role = UserRole.Technician,
                IsActive = true,
                PerformedByUserName = "admin"
            });

            var exception = await Assert.ThrowsAsync<DomainException>(action);
            Assert.Contains("собственную учетную запись", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
