using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.Services.Equipment;
using SchoolEquipmentManagement.Web.Services.Inventory;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;
using SchoolEquipmentManagement.Web.ViewModels.Inventory;

namespace SchoolEquipmentManagement.Tests.TestSupport
{
    internal static class TestEntityFactory
    {
        public static Equipment CreateEquipment(
            int id,
            string inventoryNumber = "INV-001",
            string name = "Тестовое оборудование",
            int typeId = 1,
            int statusId = 1,
            int locationId = 1,
            string statusName = "В эксплуатации")
        {
            var equipment = new Equipment(inventoryNumber, name, typeId, statusId, locationId);
            SetProperty(equipment, nameof(Equipment.Id), id);
            SetProperty(equipment, nameof(Equipment.EquipmentType), new EquipmentType("Ноутбук"));
            SetProperty(equipment, nameof(Equipment.EquipmentStatus), new EquipmentStatus(statusName));
            SetProperty(equipment, nameof(Equipment.Location), new Location("Главный корпус", "Кабинет 101"));
            return equipment;
        }

        public static InventorySession CreateSession(
            int id,
            string name = "Весенняя инвентаризация",
            DateTime? startDate = null,
            string createdBy = "Tester")
        {
            var session = new InventorySession(name, startDate ?? new DateTime(2026, 3, 29), createdBy);
            SetProperty(session, nameof(InventorySession.Id), id);
            return session;
        }

        public static InventoryRecord CreateRecord(
            int sessionId,
            int equipmentId,
            bool isFound,
            int? actualLocationId = null,
            string checkedBy = "Tester")
        {
            return new InventoryRecord(sessionId, equipmentId, isFound, checkedBy, actualLocationId);
        }

        public static ApplicationUser CreateUser(
            int id,
            string userName = "admin",
            string displayName = "Администратор",
            UserRole role = UserRole.Administrator,
            bool isActive = true,
            string passwordHash = "hashed:Admin123!")
        {
            var user = new ApplicationUser(userName, displayName, "admin@example.local", passwordHash, role, isActive);
            SetProperty(user, nameof(ApplicationUser.Id), id);
            return user;
        }

        private static void SetProperty(object target, string propertyName, object? value)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            property!.SetValue(target, value);
        }
    }

    internal sealed class FakeEquipmentRepository : IEquipmentRepository
    {
        private readonly List<Equipment> _items = new();

        public void Seed(params Equipment[] items) => _items.AddRange(items);

        public Task<List<Equipment>> GetAllAsync() => Task.FromResult(_items.OrderBy(x => x.InventoryNumber).ToList());

        public Task<Equipment?> GetByIdAsync(int id) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

        public Task<Equipment?> GetByInventoryNumberAsync(string inventoryNumber) =>
            Task.FromResult(_items.FirstOrDefault(x => x.InventoryNumber == inventoryNumber.Trim()));

        public Task<List<Equipment>> GetFilteredAsync(string? search, int? typeId, int? statusId, int? locationId) =>
            Task.FromResult(ApplyFilter(search, typeId, statusId, locationId).ToList());

        public Task<int> CountFilteredAsync(string? search, int? typeId, int? statusId, int? locationId) =>
            Task.FromResult(ApplyFilter(search, typeId, statusId, locationId).Count());

        public Task<List<Equipment>> GetFilteredPageAsync(string? search, int? typeId, int? statusId, int? locationId, int skip, int take) =>
            Task.FromResult(ApplyFilter(search, typeId, statusId, locationId).Skip(skip).Take(take).ToList());

        public Task AddAsync(Equipment equipment)
        {
            if (equipment.Id == 0)
            {
                typeof(Equipment).GetProperty(nameof(Equipment.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                    .SetValue(equipment, _items.Count + 1);
            }

            _items.Add(equipment);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Equipment equipment) => Task.CompletedTask;

        public Task<bool> ExistsByInventoryNumberAsync(string inventoryNumber) =>
            Task.FromResult(_items.Any(x => x.InventoryNumber == inventoryNumber.Trim()));

        public Task SaveChangesAsync() => Task.CompletedTask;

        private IEnumerable<Equipment> ApplyFilter(string? search, int? typeId, int? statusId, int? locationId)
        {
            var query = _items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x => x.InventoryNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                         x.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (typeId.HasValue)
            {
                query = query.Where(x => x.EquipmentTypeId == typeId.Value);
            }

            if (statusId.HasValue)
            {
                query = query.Where(x => x.EquipmentStatusId == statusId.Value);
            }

            if (locationId.HasValue)
            {
                query = query.Where(x => x.LocationId == locationId.Value);
            }

            return query.OrderBy(x => x.InventoryNumber);
        }
    }

    internal sealed class FakeEquipmentHistoryService : IEquipmentHistoryService
    {
        public List<HistoryRecordRequest> Records { get; } = new();

        public Task AddHistoryRecordAsync(int equipmentId, HistoryActionType actionType, string changedBy, string? changedField = null, string? oldValue = null, string? newValue = null, string? comment = null)
        {
            Records.Add(new HistoryRecordRequest(equipmentId, actionType, changedBy, changedField, oldValue, newValue, comment));
            return Task.CompletedTask;
        }

        public Task AddHistoryRecordsAsync(IEnumerable<HistoryRecordRequest> records)
        {
            Records.AddRange(records);
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeInventorySessionRepository : IInventorySessionRepository
    {
        private readonly List<InventorySession> _items = new();

        public void Seed(params InventorySession[] sessions) => _items.AddRange(sessions);

        public Task<List<InventorySession>> GetAllAsync() => Task.FromResult(_items.ToList());

        public Task<InventorySession?> GetByIdAsync(int id) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(InventorySession session)
        {
            if (session.Id == 0)
            {
                typeof(InventorySession).GetProperty(nameof(InventorySession.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                    .SetValue(session, _items.Count + 1);
            }

            _items.Add(session);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(InventorySession session) => Task.CompletedTask;

        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    internal sealed class FakeInventoryRecordRepository : IInventoryRecordRepository
    {
        private readonly List<InventoryRecord> _items = new();

        public List<InventoryRecord> Items => _items;

        public Task<InventoryRecord?> GetBySessionAndEquipmentAsync(int sessionId, int equipmentId) =>
            Task.FromResult(_items.FirstOrDefault(x => x.InventorySessionId == sessionId && x.EquipmentId == equipmentId));

        public Task<List<InventoryRecord>> GetLatestByEquipmentIdsAsync(IEnumerable<int> equipmentIds)
        {
            var ids = equipmentIds.Distinct().ToHashSet();

            return Task.FromResult(_items
                .Where(x => ids.Contains(x.EquipmentId))
                .GroupBy(x => x.EquipmentId)
                .Select(group => group
                    .OrderByDescending(x => x.CheckedAt)
                    .ThenByDescending(x => x.Id)
                    .First())
                .ToList());
        }

        public Task AddAsync(InventoryRecord record)
        {
            _items.Add(record);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(InventoryRecord record) => Task.CompletedTask;

        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    internal sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<ApplicationUser> _items = new();

        public void Seed(params ApplicationUser[] users) => _items.AddRange(users);

        public Task<List<ApplicationUser>> GetAllAsync() =>
            Task.FromResult(_items.OrderByDescending(x => x.IsActive).ThenBy(x => x.UserName).ToList());

        public Task<ApplicationUser?> GetByIdAsync(int id) =>
            Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

        public Task<ApplicationUser?> GetByNormalizedUserNameAsync(string normalizedUserName) =>
            Task.FromResult(_items.FirstOrDefault(x => x.NormalizedUserName == normalizedUserName));

        public Task<ApplicationUser?> GetByNormalizedEmailAsync(string normalizedEmail) =>
            Task.FromResult(_items.FirstOrDefault(x => x.NormalizedEmail == normalizedEmail));

        public Task<bool> ExistsByNormalizedUserNameAsync(string normalizedUserName) =>
            Task.FromResult(_items.Any(x => x.NormalizedUserName == normalizedUserName));

        public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail) =>
            Task.FromResult(_items.Any(x => x.NormalizedEmail == normalizedEmail));

        public Task<int> CountActiveAdministratorsAsync() =>
            Task.FromResult(_items.Count(x => x.IsActive && x.Role == UserRole.Administrator));

        public Task AddAsync(ApplicationUser user)
        {
            if (user.Id == 0)
            {
                typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                    .SetValue(user, _items.Count + 1);
            }

            _items.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ApplicationUser user) => Task.CompletedTask;

        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    internal sealed class FakePasswordHashService : IPasswordHashService
    {
        public string HashPassword(string password) => $"hashed:{password}";
    }

    internal sealed class FakeSecurityAuditService : ISecurityAuditService
    {
        public List<SecurityAuditWriteDto> Entries { get; } = new();

        public Task WriteAsync(SecurityAuditWriteDto dto, CancellationToken cancellationToken = default)
        {
            Entries.Add(dto);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SecurityAuditListItemDto>> GetRecentAsync(SecurityAuditFilterDto filter, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SecurityAuditListItemDto> result = Entries
                .Select(entry => new SecurityAuditListItemDto
                {
                    EventType = entry.EventType,
                    IsSuccessful = entry.IsSuccessful,
                    Summary = entry.Summary,
                    UserName = entry.UserName,
                    TargetUserName = entry.TargetUserName,
                    OccurredAt = DateTime.UtcNow
                })
                .ToList();

            return Task.FromResult(result);
        }
    }

    internal sealed class FakeDictionaryService : IDictionaryService
    {
        public Task<List<LookupItemDto>> GetEquipmentTypesAsync() =>
            Task.FromResult(new List<LookupItemDto> { new() { Id = 1, Name = "Ноутбук" } });

        public Task<List<LookupItemDto>> GetEquipmentStatusesAsync() =>
            Task.FromResult(new List<LookupItemDto> { new() { Id = 1, Name = "В эксплуатации" } });

        public Task<List<LookupItemDto>> GetLocationsAsync() =>
            Task.FromResult(new List<LookupItemDto> { new() { Id = 1, Name = "Главный корпус, Кабинет 101" } });
    }

    internal sealed class FakeEquipmentImportService : IEquipmentImportService
    {
        public Task<EquipmentImportPreviewDto> PreviewCsvAsync(Stream stream) => Task.FromResult(new EquipmentImportPreviewDto());

        public Task<EquipmentImportResultDto> ImportAsync(IEnumerable<EquipmentImportApplyItemDto> items, string changedBy) =>
            Task.FromResult(new EquipmentImportResultDto());
    }

    internal sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "SchoolEquipmentManagement.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    internal sealed class FakeEquipmentService : IEquipmentService
    {
        public PagedResultDto<EquipmentListItemDto> ListResult { get; set; } = new();
        public IReadOnlyList<EquipmentIssueItemDto> ProblemEquipmentResult { get; set; } = Array.Empty<EquipmentIssueItemDto>();
        public IReadOnlyList<EquipmentWarrantyItemDto> WarrantyReportResult { get; set; } = Array.Empty<EquipmentWarrantyItemDto>();
        public LocationDetailsDto? LocationDetailsResult { get; set; }
        public Dictionary<int, LocationDetailsDto> LocationDetailsById { get; } = new();
        public EquipmentDetailsDto? DetailsResult { get; set; }
        public Dictionary<int, EquipmentDetailsDto> DetailsById { get; } = new();
        public int CreatedId { get; set; } = 1;
        public CreateEquipmentDto? LastCreateDto { get; private set; }
        public UpdateEquipmentDto? LastUpdateDto { get; private set; }
        public ChangeEquipmentStatusDto? LastChangeStatusDto { get; private set; }
        public ChangeEquipmentLocationDto? LastChangeLocationDto { get; private set; }
        public AssignEquipmentResponsibleDto? LastAssignResponsibleDto { get; private set; }
        public WriteOffEquipmentDto? LastWriteOffDto { get; private set; }

        public Task<PagedResultDto<EquipmentListItemDto>> GetEquipmentListAsync(EquipmentFilterDto filter) => Task.FromResult(ListResult);
        public Task<LocationDetailsDto?> GetLocationDetailsAsync(int locationId) =>
            Task.FromResult(LocationDetailsById.TryGetValue(locationId, out var item) ? item : LocationDetailsResult);
        public Task<IReadOnlyList<EquipmentWarrantyItemDto>> GetWarrantyReportAsync(EquipmentWarrantyFilterDto filter) => Task.FromResult(WarrantyReportResult);
        public Task<IReadOnlyList<EquipmentIssueItemDto>> GetProblemEquipmentAsync() => Task.FromResult(ProblemEquipmentResult);
        public Task<EquipmentDetailsDto?> GetEquipmentDetailsAsync(int id) =>
            Task.FromResult(DetailsById.TryGetValue(id, out var item) ? item : DetailsResult);

        public Task<int> CreateEquipmentAsync(CreateEquipmentDto dto)
        {
            LastCreateDto = dto;
            return Task.FromResult(CreatedId);
        }

        public Task UpdateEquipmentAsync(UpdateEquipmentDto dto)
        {
            LastUpdateDto = dto;
            return Task.CompletedTask;
        }

        public Task ChangeStatusAsync(ChangeEquipmentStatusDto dto)
        {
            LastChangeStatusDto = dto;
            return Task.CompletedTask;
        }

        public Task ChangeLocationAsync(ChangeEquipmentLocationDto dto)
        {
            LastChangeLocationDto = dto;
            return Task.CompletedTask;
        }

        public Task AssignResponsibleAsync(AssignEquipmentResponsibleDto dto)
        {
            LastAssignResponsibleDto = dto;
            return Task.CompletedTask;
        }

        public Task WriteOffAsync(WriteOffEquipmentDto dto)
        {
            LastWriteOffDto = dto;
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeEquipmentLookupViewModelService : IEquipmentLookupViewModelService
    {
        public int PopulateFormCalls { get; private set; }
        public int PopulateIndexCalls { get; private set; }
        public int PopulateStatusOptionsCalls { get; private set; }
        public int PopulateLocationOptionsCalls { get; private set; }
        public int WrittenOffStatusId { get; set; } = 99;

        public Task PopulateFormAsync(EquipmentCreateViewModel model)
        {
            PopulateFormCalls++;
            return Task.CompletedTask;
        }

        public Task PopulateIndexAsync(EquipmentIndexViewModel model)
        {
            PopulateIndexCalls++;
            return Task.CompletedTask;
        }

        public Task PopulateStatusOptionsAsync(EquipmentChangeStatusViewModel model)
        {
            PopulateStatusOptionsCalls++;
            return Task.CompletedTask;
        }

        public Task PopulateLocationOptionsAsync(EquipmentChangeLocationViewModel model)
        {
            PopulateLocationOptionsCalls++;
            return Task.CompletedTask;
        }

        public Task<int> GetWrittenOffStatusIdAsync() => Task.FromResult(WrittenOffStatusId);
        public Task<Dictionary<string, Dictionary<string, string>>> CreateHistoryValueResolverAsync() =>
            Task.FromResult(new Dictionary<string, Dictionary<string, string>>());
    }

    internal sealed class FakeEquipmentDetailsViewModelFactory : IEquipmentDetailsViewModelFactory
    {
        public EquipmentDetailsViewModel? Result { get; set; }

        public Task<EquipmentDetailsViewModel?> CreateAsync(int id, int historyPage, bool includeFullHistory, Func<int, string> detailsUrlFactory) =>
            Task.FromResult(Result);
    }

    internal sealed class FakeEquipmentFormModelService : IEquipmentFormModelService
    {
        public EquipmentCreateViewModel HydrateCreateModel(EquipmentCreateViewModel model, IFormCollection form, ModelStateDictionary modelState) => model;
        public EquipmentEditViewModel HydrateEditModel(EquipmentEditViewModel model, IFormCollection form, ModelStateDictionary modelState) => model;
        public void ValidateEquipmentModel(EquipmentCreateViewModel model, ModelStateDictionary modelState) { }
        public void ValidatePhoto(IFormFile? photo, ModelStateDictionary modelState) { }
        public CreateEquipmentDto CreateCreateDto(EquipmentCreateViewModel model, string changedBy) => new() { InventoryNumber = model.InventoryNumber, Name = model.Name, EquipmentTypeId = 1, EquipmentStatusId = 1, LocationId = 1, ChangedBy = changedBy };
        public UpdateEquipmentDto CreateUpdateDto(EquipmentEditViewModel model, string changedBy) => new() { Id = model.Id, InventoryNumber = model.InventoryNumber, Name = model.Name, EquipmentTypeId = 1, EquipmentStatusId = 1, LocationId = 1, ChangedBy = changedBy };
    }

    internal sealed class FakeEquipmentMediaService : IEquipmentMediaService
    {
        public string PhotoSource { get; set; } = string.Empty;
        public int? LastSavedEquipmentId { get; private set; }
        public IFormFile? LastSavedPhoto { get; private set; }
        public List<int> RemovedPhotoIds { get; } = new();

        public string ResolvePhotoSource(int equipmentId, string name, string equipmentType, string inventoryNumber, bool preferUploadedFileOnly = false) => PhotoSource;
        public byte[]? GetPhotoBytes(int equipmentId) => null;

        public Task SavePhotoAsync(int equipmentId, IFormFile photo)
        {
            LastSavedEquipmentId = equipmentId;
            LastSavedPhoto = photo;
            return Task.CompletedTask;
        }

        public void RemovePhoto(int equipmentId)
        {
            RemovedPhotoIds.Add(equipmentId);
        }

        public string BuildQrCodeSource(string detailsUrl) => string.Empty;
        public byte[] BuildQrCodeBytes(string detailsUrl) => Array.Empty<byte>();
        public string BuildCodeDataUri(string inventoryNumber) => string.Empty;
        public string SanitizeFileName(string value) => value;
    }

    internal sealed class FakeInventoryService : IInventoryService
    {
        public List<InventorySessionListItemDto> Sessions { get; set; } = new();
        public InventorySessionDetailsDto? SessionDetails { get; set; }
        public InventorySessionEquipmentItemDto? CheckItem { get; set; }
        public int CreatedSessionId { get; set; } = 1;
        public CreateInventorySessionDto? LastCreateSessionDto { get; private set; }
        public int? StartedSessionId { get; private set; }
        public int? CompletedSessionId { get; private set; }
        public InventoryCheckDto? LastInventoryCheckDto { get; private set; }

        public Task<List<InventorySessionListItemDto>> GetSessionsAsync() => Task.FromResult(Sessions);
        public Task<InventorySessionDetailsDto?> GetSessionDetailsAsync(int id) => Task.FromResult(SessionDetails);

        public Task<int> CreateSessionAsync(CreateInventorySessionDto dto)
        {
            LastCreateSessionDto = dto;
            return Task.FromResult(CreatedSessionId);
        }

        public Task StartSessionAsync(int id)
        {
            StartedSessionId = id;
            return Task.CompletedTask;
        }

        public Task CompleteSessionAsync(int id)
        {
            CompletedSessionId = id;
            return Task.CompletedTask;
        }

        public Task<InventorySessionEquipmentItemDto?> GetCheckItemAsync(int sessionId, int equipmentId) => Task.FromResult(CheckItem);

        public Task RecordCheckAsync(InventoryCheckDto dto)
        {
            LastInventoryCheckDto = dto;
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeInventoryLookupViewModelService : IInventoryLookupViewModelService
    {
        public int PopulateLocationsCalls { get; private set; }

        public Task PopulateLocationsAsync(InventoryCheckViewModel model)
        {
            PopulateLocationsCalls++;
            return Task.CompletedTask;
        }
    }

    internal sealed class DictionaryTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    internal static class ControllerTestHelper
    {
        public static TempDataDictionary CreateTempData()
        {
            return new TempDataDictionary(new DefaultHttpContext(), new DictionaryTempDataProvider());
        }

        public static ControllerContext CreateControllerContext()
        {
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData()
            };
        }
    }
}
