using Microsoft.AspNetCore.Http;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public interface IEquipmentMediaService
{
    string ResolvePhotoSource(int equipmentId, string name, string equipmentType, string inventoryNumber, bool preferUploadedFileOnly = false);
    byte[]? GetPhotoBytes(int equipmentId);
    Task SavePhotoAsync(int equipmentId, IFormFile photo);
    void RemovePhoto(int equipmentId);
    string BuildQrCodeSource(string detailsUrl);
    byte[] BuildQrCodeBytes(string detailsUrl);
    string BuildCodeDataUri(string inventoryNumber);
    string SanitizeFileName(string value);
}
