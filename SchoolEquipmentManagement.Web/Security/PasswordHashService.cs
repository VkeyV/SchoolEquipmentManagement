using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Infrastructure.Security;

namespace SchoolEquipmentManagement.Web.Security
{
    public sealed class PasswordHashService : IPasswordHashService
    {
        public string HashPassword(string password)
        {
            return PasswordHashUtility.HashPassword(password);
        }
    }
}
