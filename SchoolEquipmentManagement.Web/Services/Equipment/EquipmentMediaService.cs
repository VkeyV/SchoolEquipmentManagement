using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using QRCoder;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public sealed class EquipmentMediaService : IEquipmentMediaService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EquipmentMediaService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public string ResolvePhotoSource(int equipmentId, string name, string equipmentType, string inventoryNumber, bool preferUploadedFileOnly = false)
    {
        var existingPhotoUrl = GetPhotoUrl(equipmentId);
        if (!string.IsNullOrWhiteSpace(existingPhotoUrl))
        {
            return existingPhotoUrl;
        }

        return preferUploadedFileOnly
            ? string.Empty
            : BuildPhotoDataUri(name, equipmentType, inventoryNumber);
    }

    public byte[]? GetPhotoBytes(int equipmentId)
    {
        var existingFile = GetPhotoPath(equipmentId);
        return existingFile is null ? null : File.ReadAllBytes(existingFile);
    }

    public async Task SavePhotoAsync(int equipmentId, IFormFile photo)
    {
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var uploadsPath = GetUploadsPath();

        Directory.CreateDirectory(uploadsPath);
        RemovePhoto(equipmentId);

        var fileName = $"equipment-{equipmentId}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await photo.CopyToAsync(stream);
    }

    public void RemovePhoto(int equipmentId)
    {
        var uploadsPath = GetUploadsPath();
        if (!Directory.Exists(uploadsPath))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(uploadsPath, $"equipment-{equipmentId}.*", SearchOption.TopDirectoryOnly))
        {
            File.Delete(file);
        }
    }

    public string BuildQrCodeSource(string detailsUrl)
    {
        var qrBytes = BuildQrCodeBytes(detailsUrl);
        return $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";
    }

    public byte[] BuildQrCodeBytes(string detailsUrl)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(detailsUrl, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(28, drawQuietZones: false);
    }

    public string BuildCodeDataUri(string inventoryNumber)
    {
        const int moduleSize = 8;
        const int quietZone = 16;
        const int size = 21;
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(inventoryNumber));
        var builder = new StringBuilder();

        builder.Append("""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 220 248" role="img" aria-label="Код объекта">
              <rect width="220" height="248" rx="20" fill="#ffffff"/>
              <rect x="18" y="18" width="184" height="184" rx="18" fill="#f6f8fb" stroke="rgba(22,32,51,0.08)"/>
            """);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                if (!ShouldFillCodeModule(bytes, x, y, size))
                {
                    continue;
                }

                var rectX = quietZone + (x * moduleSize) + 18;
                var rectY = quietZone + (y * moduleSize) + 18;
                builder.Append($"<rect x=\"{rectX}\" y=\"{rectY}\" width=\"{moduleSize}\" height=\"{moduleSize}\" rx=\"1\" fill=\"#162033\"/>");
            }
        }

        var encodedInventoryNumber = WebUtility.HtmlEncode(inventoryNumber);
        builder.Append($"<text x=\"110\" y=\"226\" text-anchor=\"middle\" fill=\"#4f6486\" font-size=\"16\" font-family=\"Segoe UI, Arial\">{encodedInventoryNumber}</text>");
        builder.Append("</svg>");

        return ConvertSvgToDataUri(builder.ToString());
    }

    public string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var symbol in value)
        {
            builder.Append(invalidChars.Contains(symbol) ? '-' : symbol);
        }

        return builder.ToString();
    }

    private string? GetPhotoUrl(int equipmentId)
    {
        var existingFile = GetPhotoPath(equipmentId);
        return existingFile is null ? null : $"/uploads/equipment/{Path.GetFileName(existingFile)}";
    }

    private string? GetPhotoPath(int equipmentId)
    {
        var uploadsPath = GetUploadsPath();
        if (!Directory.Exists(uploadsPath))
        {
            return null;
        }

        return Directory
            .EnumerateFiles(uploadsPath, $"equipment-{equipmentId}.*", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path)
            .FirstOrDefault();
    }

    private string GetUploadsPath() =>
        Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "equipment");

    private static string BuildPhotoDataUri(string name, string equipmentType, string inventoryNumber)
    {
        var title = WebUtility.HtmlEncode(name);
        var subtitle = WebUtility.HtmlEncode(equipmentType);
        var code = WebUtility.HtmlEncode(inventoryNumber);

        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 420 280" role="img" aria-label="{title}">
              <defs>
                <linearGradient id="cardBg" x1="0" x2="1" y1="0" y2="1">
                  <stop offset="0%" stop-color="#f5f8fd" />
                  <stop offset="100%" stop-color="#dce8fb" />
                </linearGradient>
              </defs>
              <rect width="420" height="280" rx="28" fill="url(#cardBg)" />
              <rect x="24" y="24" width="372" height="232" rx="22" fill="rgba(255,255,255,0.76)" stroke="rgba(22,32,51,0.08)" />
              <rect x="44" y="44" width="96" height="96" rx="24" fill="#1f4b99" />
              <text x="92" y="103" text-anchor="middle" fill="#ffffff" font-size="36" font-family="Segoe UI, Arial" font-weight="700">IT</text>
              <text x="44" y="172" fill="#162033" font-size="24" font-family="Segoe UI, Arial" font-weight="700">{title}</text>
              <text x="44" y="204" fill="#4f6486" font-size="18" font-family="Segoe UI, Arial">{subtitle}</text>
              <text x="44" y="232" fill="#667085" font-size="16" font-family="Segoe UI, Arial">Инвентарный номер: {code}</text>
            </svg>
            """;

        return ConvertSvgToDataUri(svg);
    }

    private static bool ShouldFillCodeModule(byte[] bytes, int x, int y, int size)
    {
        if (IsFinderZone(x, y, 0, 0) || IsFinderZone(x, y, size - 7, 0) || IsFinderZone(x, y, 0, size - 7))
        {
            return IsFinderRing(x, y, 0, 0) || IsFinderRing(x, y, size - 7, 0) || IsFinderRing(x, y, 0, size - 7);
        }

        var index = (x * 13 + y * 17 + x * y) % bytes.Length;
        var bit = (x + y) % 8;
        return ((bytes[index] >> bit) & 1) == 1;
    }

    private static bool IsFinderZone(int x, int y, int startX, int startY) =>
        x >= startX && x < startX + 7 && y >= startY && y < startY + 7;

    private static bool IsFinderRing(int x, int y, int startX, int startY)
    {
        if (!IsFinderZone(x, y, startX, startY))
        {
            return false;
        }

        var localX = x - startX;
        var localY = y - startY;
        var onOuter = localX is 0 or 6 || localY is 0 or 6;
        var onInner = localX is >= 2 and <= 4 && localY is >= 2 and <= 4;
        return onOuter || onInner;
    }

    private static string ConvertSvgToDataUri(string svg) =>
        $"data:image/svg+xml;utf8,{Uri.EscapeDataString(svg)}";
}
