using Microsoft.AspNetCore.Http;
using SchoolEquipmentManagement.Tests.TestSupport;
using SchoolEquipmentManagement.Web.Services.Equipment;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class EquipmentMediaServiceTests
    {
        [Fact]
        public async Task SavePhotoAsync_AndGetPhotoBytes_ShouldPersistUploadedFile()
        {
            var root = CreateTempRoot();

            try
            {
                var service = CreateService(root);
                var bytes = new byte[] { 10, 20, 30, 40 };
                await using var stream = new MemoryStream(bytes);
                IFormFile photo = new FormFile(stream, 0, stream.Length, "Photo", "device.png");

                await service.SavePhotoAsync(15, photo);

                var stored = service.GetPhotoBytes(15);
                Assert.NotNull(stored);
                Assert.Equal(bytes, stored);
                Assert.Equal("/uploads/equipment/equipment-15.png", service.ResolvePhotoSource(15, "Name", "Type", "INV-15"));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }

        [Fact]
        public async Task SavePhotoAsync_ShouldReplaceExistingPhotoWithNewExtension()
        {
            var root = CreateTempRoot();

            try
            {
                var service = CreateService(root);
                await SavePhotoAsync(service, 8, "first.jpg", new byte[] { 1, 2, 3 });
                await SavePhotoAsync(service, 8, "second.webp", new byte[] { 4, 5, 6 });

                var uploadsPath = Path.Combine(root, "uploads", "equipment");
                var files = Directory.GetFiles(uploadsPath, "equipment-8.*");
                Assert.Single(files);
                Assert.EndsWith("equipment-8.webp", files[0], StringComparison.OrdinalIgnoreCase);
                Assert.Equal(new byte[] { 4, 5, 6 }, service.GetPhotoBytes(8));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }

        [Fact]
        public void ResolvePhotoSource_ShouldReturnFallbackOrEmpty_WhenUploadedFileMissing()
        {
            var root = CreateTempRoot();

            try
            {
                var service = CreateService(root);

                var fallback = service.ResolvePhotoSource(3, "Рабочая станция", "Ноутбук", "INV<03>");
                var uploadedOnly = service.ResolvePhotoSource(3, "Рабочая станция", "Ноутбук", "INV<03>", preferUploadedFileOnly: true);

                Assert.StartsWith("data:image/svg+xml;utf8,", fallback);
                Assert.Equal(string.Empty, uploadedOnly);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }

        [Fact]
        public void BuildQrCodeAndCodeDataUri_ShouldReturnDataUris()
        {
            var root = CreateTempRoot();

            try
            {
                var service = CreateService(root);

                var qrBytes = service.BuildQrCodeBytes("https://example.test/equipment/5");
                var qrDataUri = service.BuildQrCodeSource("https://example.test/equipment/5");
                var codeDataUri = service.BuildCodeDataUri("INV&005");
                var svg = Uri.UnescapeDataString(codeDataUri["data:image/svg+xml;utf8,".Length..]);

                Assert.NotEmpty(qrBytes);
                Assert.StartsWith("data:image/png;base64,", qrDataUri);
                Assert.StartsWith("data:image/svg+xml;utf8,", codeDataUri);
                Assert.Contains("INV&amp;005", svg);
                Assert.Contains("aria-label=\"Код объекта\"", svg);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }

        [Fact]
        public async Task RemovePhoto_AndSanitizeFileName_ShouldCleanupAndNormalize()
        {
            var root = CreateTempRoot();

            try
            {
                var service = CreateService(root);
                await SavePhotoAsync(service, 22, "bad:name?.png", new byte[] { 9, 9, 9 });

                Assert.NotNull(service.GetPhotoBytes(22));
                service.RemovePhoto(22);

                Assert.Null(service.GetPhotoBytes(22));
                Assert.DoesNotContain(':', service.SanitizeFileName("report:2026?.png"));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }

        private static EquipmentMediaService CreateService(string webRootPath) =>
            new(new FakeWebHostEnvironment { WebRootPath = webRootPath });

        private static string CreateTempRoot()
        {
            var path = Path.Combine(Path.GetTempPath(), "SchoolEquipmentManagement.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static async Task SavePhotoAsync(EquipmentMediaService service, int equipmentId, string fileName, byte[] bytes)
        {
            await using var stream = new MemoryStream(bytes);
            IFormFile photo = new FormFile(stream, 0, stream.Length, "Photo", fileName);
            await service.SavePhotoAsync(equipmentId, photo);
        }
    }
}
