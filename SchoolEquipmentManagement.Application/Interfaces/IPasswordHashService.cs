namespace SchoolEquipmentManagement.Application.Interfaces
{
    public interface IPasswordHashService
    {
        string HashPassword(string password);
    }
}
