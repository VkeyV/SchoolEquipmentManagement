using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;


namespace SchoolEquipmentManagement.Application.Services
{
    public class EquipmentService : IEquipmentService
    {
        private const string RepairStatus = "В ремонте";
        private const string DiagnosticsStatus = "Требует диагностики";

        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IInventoryRecordRepository _inventoryRecordRepository;
        private readonly IEquipmentHistoryService _equipmentHistoryService;

        public EquipmentService(
            IEquipmentRepository equipmentRepository,
            IInventoryRecordRepository inventoryRecordRepository,
            IEquipmentHistoryService equipmentHistoryService)
        {
            _equipmentRepository = equipmentRepository;
            _inventoryRecordRepository = inventoryRecordRepository;
            _equipmentHistoryService = equipmentHistoryService;
        }

        public async Task<PagedResultDto<EquipmentListItemDto>> GetEquipmentListAsync(EquipmentFilterDto filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            if (!string.IsNullOrWhiteSpace(filter.WarrantyFilter))
            {
                var filteredItems = await GetWarrantyAwareEquipmentAsync(filter);
                var filteredTotalCount = filteredItems.Count;
                var filteredTotalPages = filteredTotalCount == 0
                    ? 1
                    : (int)Math.Ceiling((double)filteredTotalCount / pageSize);

                if (page > filteredTotalPages)
                {
                    page = filteredTotalPages;
                }

                var pagedItems = filteredItems
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PagedResultDto<EquipmentListItemDto>
                {
                    Items = pagedItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = filteredTotalCount
                };
            }

            var totalCount = await _equipmentRepository.CountFilteredAsync(
                filter.Search,
                filter.TypeId,
                filter.StatusId,
                filter.LocationId);

            var totalPages = totalCount == 0
                ? 1
                : (int)Math.Ceiling((double)totalCount / pageSize);

            if (page > totalPages)
            {
                page = totalPages;
            }

            var skip = (page - 1) * pageSize;

            var equipmentItems = await _equipmentRepository.GetFilteredPageAsync(
                filter.Search,
                filter.TypeId,
                filter.StatusId,
                filter.LocationId,
                skip,
                pageSize);

            var items = equipmentItems
                .Select(MapEquipmentListItem)
                .ToList();

            return new PagedResultDto<EquipmentListItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<LocationDetailsDto?> GetLocationDetailsAsync(int locationId)
        {
            var equipmentItems = await _equipmentRepository.GetAllAsync();
            var assignedEquipment = equipmentItems
                .Where(item => item.LocationId == locationId)
                .OrderBy(item => item.InventoryNumber)
                .ToList();

            var latestRecords = equipmentItems.Count == 0
                ? new List<InventoryRecord>()
                : await _inventoryRecordRepository.GetLatestByEquipmentIdsAsync(equipmentItems.Select(item => item.Id));

            var locationReference = assignedEquipment
                .Select(item => item.Location)
                .FirstOrDefault()
                ?? latestRecords
                    .Where(record => record.ActualLocationId == locationId)
                    .Select(record => record.ActualLocation)
                    .FirstOrDefault(location => location is not null);

            if (locationReference is null)
            {
                return null;
            }

            var latestRecordsByEquipmentId = latestRecords.ToDictionary(record => record.EquipmentId);
            var inventoryDiscrepancies = BuildLocationInventoryDiscrepancies(locationId, equipmentItems, latestRecordsByEquipmentId);
            var relatedInventoryChecks = latestRecords
                .Where(record => RecordRelatesToLocation(locationId, equipmentItems, record))
                .Select(record => (DateTime?)record.CheckedAt)
                .OrderByDescending(value => value)
                .FirstOrDefault();

            return new LocationDetailsDto
            {
                Id = locationId,
                Name = locationReference.GetDisplayName(),
                Building = locationReference.Building,
                Room = locationReference.Room,
                Description = locationReference.Description,
                EquipmentCount = assignedEquipment.Count,
                DiscrepancyCount = inventoryDiscrepancies.Count,
                MissingCount = inventoryDiscrepancies.Count(item => item.DiscrepancyCode == "missing"),
                LastInventoryCheckedAt = relatedInventoryChecks,
                StatusSummary = assignedEquipment
                    .GroupBy(item => item.EquipmentStatus.Name)
                    .OrderByDescending(group => group.Count())
                    .ThenBy(group => group.Key)
                    .Select(group => new LocationStatusSummaryDto
                    {
                        StatusName = group.Key,
                        Count = group.Count()
                    })
                    .ToList(),
                EquipmentItems = assignedEquipment
                    .Select(item => MapLocationEquipmentItem(item, latestRecordsByEquipmentId.TryGetValue(item.Id, out var latestRecord) ? latestRecord : null))
                    .ToList(),
                InventoryDiscrepancies = inventoryDiscrepancies
            };
        }

        public async Task<IReadOnlyList<EquipmentWarrantyItemDto>> GetWarrantyReportAsync(EquipmentWarrantyFilterDto filter)
        {
            var equipmentItems = await _equipmentRepository.GetAllAsync();

            return equipmentItems
                .Where(item => MatchesWarrantyFilter(item.WarrantyEndDate, filter.WarrantyFilter))
                .OrderBy(item => GetWarrantySortOrder(item.WarrantyEndDate))
                .ThenBy(item => item.WarrantyEndDate ?? DateTime.MaxValue)
                .ThenBy(item => item.InventoryNumber)
                .Select(item => new EquipmentWarrantyItemDto
                {
                    Id = item.Id,
                    InventoryNumber = item.InventoryNumber,
                    Name = item.Name,
                    EquipmentTypeName = item.EquipmentType.Name,
                    EquipmentStatusName = item.EquipmentStatus.Name,
                    LocationName = item.Location.GetDisplayName(),
                    ResponsiblePerson = item.ResponsiblePerson,
                    WarrantyEndDate = item.WarrantyEndDate,
                    WarrantyDaysLeft = GetWarrantyDaysLeft(item.WarrantyEndDate)
                })
                .ToList();
        }

        public async Task<IReadOnlyList<EquipmentIssueItemDto>> GetProblemEquipmentAsync()
        {
            var equipmentItems = await _equipmentRepository.GetAllAsync();
            if (equipmentItems.Count == 0)
            {
                return Array.Empty<EquipmentIssueItemDto>();
            }

            var latestRecords = await _inventoryRecordRepository.GetLatestByEquipmentIdsAsync(equipmentItems.Select(x => x.Id));
            var latestRecordsByEquipmentId = latestRecords.ToDictionary(x => x.EquipmentId);
            var issues = new List<EquipmentIssueItemDto>();

            foreach (var equipment in equipmentItems)
            {
                if (string.Equals(equipment.EquipmentStatus.Name, RepairStatus, StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(CreateIssue(
                        equipment,
                        issueCode: "repair",
                        issueTitle: "В ремонте",
                        issueDescription: "Оборудование выведено из эксплуатации и находится в ремонте.",
                        priority: 2,
                        priorityLabel: "Средний"));
                }

                if (string.Equals(equipment.EquipmentStatus.Name, DiagnosticsStatus, StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(CreateIssue(
                        equipment,
                        issueCode: "diagnostics",
                        issueTitle: "Требует диагностики",
                        issueDescription: "Оборудование требует диагностики и решения по дальнейшему обслуживанию.",
                        priority: 3,
                        priorityLabel: "Высокий"));
                }

                if (!latestRecordsByEquipmentId.TryGetValue(equipment.Id, out var latestRecord))
                {
                    continue;
                }

                if (!latestRecord.IsFound)
                {
                    issues.Add(CreateIssue(
                        equipment,
                        issueCode: "missing",
                        issueTitle: "Не найдено",
                        issueDescription: BuildMissingIssueDescription(latestRecord),
                        priority: 4,
                        priorityLabel: "Критичный",
                        latestRecord));

                    continue;
                }

                if (latestRecord.ActualLocationId.HasValue && latestRecord.ActualLocationId.Value != equipment.LocationId)
                {
                    issues.Add(CreateIssue(
                        equipment,
                        issueCode: "discrepancy",
                        issueTitle: "Найдено с расхождением",
                        issueDescription: BuildDiscrepancyIssueDescription(equipment.Location.GetDisplayName(), latestRecord.ActualLocation?.GetDisplayName()),
                        priority: 3,
                        priorityLabel: "Высокий",
                        latestRecord));
                }
            }

            return issues
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.IssueTitle)
                .ThenBy(x => x.InventoryNumber)
                .ToList();
        }

        public async Task<EquipmentDetailsDto?> GetEquipmentDetailsAsync(int id)
        {
            var equipment = await _equipmentRepository.GetByIdAsync(id);
            if (equipment is null)
                return null;

            return new EquipmentDetailsDto
            {
                Id = equipment.Id,
                InventoryNumber = equipment.InventoryNumber,
                Name = equipment.Name,
                SerialNumber = equipment.SerialNumber,
                Model = equipment.Model,
                Manufacturer = equipment.Manufacturer,
                PurchaseDate = equipment.PurchaseDate,
                CommissioningDate = equipment.CommissioningDate,
                WarrantyEndDate = equipment.WarrantyEndDate,
                EquipmentTypeName = equipment.EquipmentType.Name,
                EquipmentStatusName = equipment.EquipmentStatus.Name,
                LocationName = equipment.Location.GetDisplayName(),
                ResponsiblePerson = equipment.ResponsiblePerson,
                Notes = equipment.Notes,
                EquipmentTypeId = equipment.EquipmentTypeId,
                EquipmentStatusId = equipment.EquipmentStatusId,
                LocationId = equipment.LocationId,
                History = equipment.HistoryEntries
                    .OrderByDescending(h => h.ChangedAt)
                    .Select(h => new EquipmentHistoryItemDto
                    {
                        Id = h.Id,
                        ActionType = h.ActionType.ToString(),
                        ChangedField = h.ChangedField,
                        OldValue = h.OldValue,
                        NewValue = h.NewValue,
                        Comment = h.Comment,
                        ChangedBy = h.ChangedBy,
                        ChangedAt = h.ChangedAt
                    })
                    .ToList()
            };
        }

        public async Task<int> CreateEquipmentAsync(CreateEquipmentDto dto)
        {
            ValidateAuditUser(dto.ChangedBy);
            await ValidateDuplicateInventoryNumberAsync(dto.InventoryNumber);

            var equipment = new Equipment(
                dto.InventoryNumber,
                dto.Name,
                dto.EquipmentTypeId,
                dto.EquipmentStatusId,
                dto.LocationId,
                dto.SerialNumber,
                dto.Model,
                dto.Manufacturer,
                dto.PurchaseDate,
                dto.CommissioningDate,
                dto.WarrantyEndDate,
                dto.ResponsiblePerson,
                dto.Notes);

            await _equipmentRepository.AddAsync(equipment);
            await _equipmentRepository.SaveChangesAsync();

            await _equipmentHistoryService.AddHistoryRecordAsync(
                equipment.Id,
                HistoryActionType.Created,
                dto.ChangedBy,
                comment: "Создана новая карточка оборудования.");

            return equipment.Id;
        }

        public async Task UpdateEquipmentAsync(UpdateEquipmentDto dto)
        {
            ValidateAuditUser(dto.ChangedBy);
            var equipment = await _equipmentRepository.GetByIdAsync(dto.Id)
                ?? throw new DomainException("Оборудование не найдено.");

            var existingWithSameNumber = await _equipmentRepository.GetByInventoryNumberAsync(dto.InventoryNumber);
            if (existingWithSameNumber is not null && existingWithSameNumber.Id != dto.Id)
                throw new DomainException("Оборудование с таким инвентарным номером уже существует.");

            var oldInventoryNumber = equipment.InventoryNumber;
            var oldName = equipment.Name;
            var oldSerialNumber = equipment.SerialNumber;
            var oldManufacturer = equipment.Manufacturer;
            var oldModel = equipment.Model;
            var oldPurchaseDate = equipment.PurchaseDate;
            var oldCommissioningDate = equipment.CommissioningDate;
            var oldWarrantyEndDate = equipment.WarrantyEndDate;
            var oldResponsiblePerson = equipment.ResponsiblePerson;
            var oldNotes = equipment.Notes;
            var oldTypeId = equipment.EquipmentTypeId;
            var oldStatusId = equipment.EquipmentStatusId;
            var oldLocationId = equipment.LocationId;

            equipment.UpdateCard(
                dto.InventoryNumber,
                dto.Name,
                dto.EquipmentTypeId,
                dto.EquipmentStatusId,
                dto.LocationId,
                dto.SerialNumber,
                dto.Model,
                dto.Manufacturer,
                dto.PurchaseDate,
                dto.CommissioningDate,
                dto.WarrantyEndDate,
                dto.ResponsiblePerson,
                dto.Notes);

            await _equipmentRepository.UpdateAsync(equipment);
            await _equipmentRepository.SaveChangesAsync();

            await WriteUpdateHistoryAsync(equipment.Id, dto.ChangedBy,
                oldInventoryNumber, dto.InventoryNumber,
                oldName, dto.Name,
                oldSerialNumber, dto.SerialNumber,
                oldManufacturer, dto.Manufacturer,
                oldModel, dto.Model,
                oldPurchaseDate, dto.PurchaseDate,
                oldCommissioningDate, dto.CommissioningDate,
                oldWarrantyEndDate, dto.WarrantyEndDate,
                oldResponsiblePerson, dto.ResponsiblePerson,
                oldNotes, dto.Notes,
                oldTypeId, dto.EquipmentTypeId,
                oldStatusId, dto.EquipmentStatusId,
                oldLocationId, dto.LocationId);
        }

        public async Task ChangeStatusAsync(ChangeEquipmentStatusDto dto)
        {
            ValidateAuditUser(dto.ChangedBy);
            var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId)
                ?? throw new DomainException("Оборудование не найдено.");

            EnsureNotWrittenOff(equipment, "изменить статус");

            var oldStatusId = equipment.EquipmentStatusId;
            if (oldStatusId == dto.NewStatusId)
                return;

            equipment.ChangeStatus(dto.NewStatusId);

            await _equipmentRepository.UpdateAsync(equipment);
            await _equipmentRepository.SaveChangesAsync();

            await _equipmentHistoryService.AddHistoryRecordAsync(
                equipment.Id,
                HistoryActionType.StatusChanged,
                dto.ChangedBy,
                changedField: "EquipmentStatusId",
                oldValue: oldStatusId.ToString(),
                newValue: dto.NewStatusId.ToString(),
                comment: dto.Comment ?? "Изменен статус оборудования.");
        }

        public async Task ChangeLocationAsync(ChangeEquipmentLocationDto dto)
        {
            ValidateAuditUser(dto.ChangedBy);
            var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId)
                ?? throw new DomainException("Оборудование не найдено.");

            EnsureNotWrittenOff(equipment, "изменить местоположение");

            var oldLocationId = equipment.LocationId;
            if (oldLocationId == dto.NewLocationId)
                return;

            equipment.ChangeLocation(dto.NewLocationId);

            await _equipmentRepository.UpdateAsync(equipment);
            await _equipmentRepository.SaveChangesAsync();

            await _equipmentHistoryService.AddHistoryRecordAsync(
                equipment.Id,
                HistoryActionType.LocationChanged,
                dto.ChangedBy,
                changedField: "LocationId",
                oldValue: oldLocationId.ToString(),
                newValue: dto.NewLocationId.ToString(),
                comment: dto.Comment ?? "Изменено местоположение оборудования.");
        }

        public async Task AssignResponsibleAsync(AssignEquipmentResponsibleDto dto)
        {
            ValidateAuditUser(dto.ChangedBy);

            var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId)
                ?? throw new DomainException("Оборудование не найдено.");

            EnsureNotWrittenOff(equipment, "назначить ответственного");

            var oldResponsiblePerson = equipment.ResponsiblePerson;
            var newResponsiblePerson = NormalizeText(dto.ResponsiblePerson);

            if (string.Equals(oldResponsiblePerson, newResponsiblePerson, StringComparison.Ordinal))
            {
                return;
            }

            equipment.ChangeResponsiblePerson(newResponsiblePerson);

            await _equipmentRepository.UpdateAsync(equipment);
            await _equipmentRepository.SaveChangesAsync();

            await _equipmentHistoryService.AddHistoryRecordAsync(
                equipment.Id,
                HistoryActionType.Updated,
                dto.ChangedBy,
                changedField: "ResponsiblePerson",
                oldValue: oldResponsiblePerson,
                newValue: newResponsiblePerson,
                comment: dto.Comment ?? "Обновлено ответственное лицо.");
        }

        public async Task WriteOffAsync(WriteOffEquipmentDto dto)
        {
            ValidateAuditUser(dto.ChangedBy);
            var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId)
                ?? throw new DomainException("Оборудование не найдено.");

            if (IsWrittenOff(equipment))
            {
                throw new DomainException("Оборудование уже списано.");
            }

            var oldStatusId = equipment.EquipmentStatusId;
            equipment.ChangeStatus(dto.WrittenOffStatusId);

            await _equipmentRepository.UpdateAsync(equipment);
            await _equipmentRepository.SaveChangesAsync();

            await _equipmentHistoryService.AddHistoryRecordAsync(
                equipment.Id,
                HistoryActionType.WrittenOff,
                dto.ChangedBy,
                changedField: "EquipmentStatusId",
                oldValue: oldStatusId.ToString(),
                newValue: dto.WrittenOffStatusId.ToString(),
                comment: dto.Comment ?? "Оборудование списано.");
        }

        private async Task ValidateDuplicateInventoryNumberAsync(string inventoryNumber)
        {
            inventoryNumber = inventoryNumber.Trim();
            var exists = await _equipmentRepository.ExistsByInventoryNumberAsync(inventoryNumber);
            if (exists)
                throw new DomainException("Оборудование с таким инвентарным номером уже существует.");
        }

        private static void ValidateAuditUser(string changedBy)
        {
            if (string.IsNullOrWhiteSpace(changedBy))
                throw new DomainException("Не указан пользователь, выполнивший операцию.");
        }

        private static void EnsureNotWrittenOff(Equipment equipment, string actionDescription)
        {
            if (IsWrittenOff(equipment))
            {
                throw new DomainException($"Нельзя {actionDescription} у списанного оборудования.");
            }
        }

        private static bool IsWrittenOff(Equipment equipment)
        {
            return string.Equals(
                equipment.EquipmentStatus?.Name,
                "Списано",
                StringComparison.OrdinalIgnoreCase);
        }

        private static EquipmentIssueItemDto CreateIssue(
            Equipment equipment,
            string issueCode,
            string issueTitle,
            string issueDescription,
            int priority,
            string priorityLabel,
            InventoryRecord? latestRecord = null)
        {
            return new EquipmentIssueItemDto
            {
                EquipmentId = equipment.Id,
                InventoryNumber = equipment.InventoryNumber,
                Name = equipment.Name,
                EquipmentTypeName = equipment.EquipmentType.Name,
                EquipmentStatusName = equipment.EquipmentStatus.Name,
                LocationName = equipment.Location.GetDisplayName(),
                ResponsiblePerson = equipment.ResponsiblePerson,
                IssueCode = issueCode,
                IssueTitle = issueTitle,
                IssueDescription = issueDescription,
                Priority = priority,
                PriorityLabel = priorityLabel,
                LastCheckedAt = latestRecord?.CheckedAt,
                LastCheckedBy = latestRecord?.CheckedBy,
                ExpectedLocationName = equipment.Location.GetDisplayName(),
                ActualLocationName = latestRecord?.ActualLocation?.GetDisplayName()
            };
        }

        private static string BuildMissingIssueDescription(InventoryRecord latestRecord)
        {
            var checkedAt = latestRecord.CheckedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
            return $"Последняя инвентаризация не подтвердила наличие объекта. Проверка: {checkedAt}.";
        }

        private static string BuildDiscrepancyIssueDescription(string expectedLocationName, string? actualLocationName)
        {
            var actualLocationText = string.IsNullOrWhiteSpace(actualLocationName)
                ? "фактическое местоположение не указано"
                : actualLocationName;

            return $"По последней инвентаризации объект найден не там, где ожидалось. Ожидалось: {expectedLocationName}. Фактически: {actualLocationText}.";
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private async Task<List<EquipmentListItemDto>> GetWarrantyAwareEquipmentAsync(EquipmentFilterDto filter)
        {
            var equipmentItems = await _equipmentRepository.GetFilteredAsync(
                filter.Search,
                filter.TypeId,
                filter.StatusId,
                filter.LocationId);

            return equipmentItems
                .Where(item => MatchesWarrantyFilter(item.WarrantyEndDate, filter.WarrantyFilter))
                .OrderBy(item => GetWarrantySortOrder(item.WarrantyEndDate))
                .ThenBy(item => item.WarrantyEndDate ?? DateTime.MaxValue)
                .ThenBy(item => item.InventoryNumber)
                .Select(MapEquipmentListItem)
                .ToList();
        }

        private static EquipmentListItemDto MapEquipmentListItem(Equipment equipment)
        {
            return new EquipmentListItemDto
            {
                Id = equipment.Id,
                LocationId = equipment.LocationId,
                InventoryNumber = equipment.InventoryNumber,
                Name = equipment.Name,
                EquipmentTypeName = equipment.EquipmentType.Name,
                EquipmentStatusName = equipment.EquipmentStatus.Name,
                LocationName = equipment.Location.GetDisplayName(),
                ResponsiblePerson = equipment.ResponsiblePerson,
                WarrantyEndDate = equipment.WarrantyEndDate,
                WarrantyDaysLeft = GetWarrantyDaysLeft(equipment.WarrantyEndDate)
            };
        }

        private static LocationEquipmentItemDto MapLocationEquipmentItem(Equipment equipment, InventoryRecord? latestRecord)
        {
            return new LocationEquipmentItemDto
            {
                Id = equipment.Id,
                InventoryNumber = equipment.InventoryNumber,
                Name = equipment.Name,
                EquipmentTypeName = equipment.EquipmentType.Name,
                EquipmentStatusName = equipment.EquipmentStatus.Name,
                ResponsiblePerson = equipment.ResponsiblePerson,
                WarrantyEndDate = equipment.WarrantyEndDate,
                WarrantyDaysLeft = GetWarrantyDaysLeft(equipment.WarrantyEndDate),
                LastInventoryStatus = BuildLocationInventoryStatus(equipment, latestRecord),
                LastInventoryCheckedAt = latestRecord?.CheckedAt,
                LastInventoryCheckedBy = latestRecord?.CheckedBy
            };
        }

        private static List<LocationInventoryDiscrepancyDto> BuildLocationInventoryDiscrepancies(
            int locationId,
            IReadOnlyList<Equipment> equipmentItems,
            IReadOnlyDictionary<int, InventoryRecord> latestRecordsByEquipmentId)
        {
            var discrepancies = new List<LocationInventoryDiscrepancyDto>();

            foreach (var equipment in equipmentItems)
            {
                if (!latestRecordsByEquipmentId.TryGetValue(equipment.Id, out var latestRecord))
                {
                    continue;
                }

                if (equipment.LocationId == locationId)
                {
                    if (!latestRecord.IsFound)
                    {
                        discrepancies.Add(new LocationInventoryDiscrepancyDto
                        {
                            EquipmentId = equipment.Id,
                            InventoryNumber = equipment.InventoryNumber,
                            Name = equipment.Name,
                            EquipmentTypeName = equipment.EquipmentType.Name,
                            EquipmentStatusName = equipment.EquipmentStatus.Name,
                            ExpectedLocationName = equipment.Location.GetDisplayName(),
                            DiscrepancyCode = "missing",
                            DiscrepancyTitle = "Не найдено в локации",
                            DiscrepancySummary = BuildMissingIssueDescription(latestRecord),
                            CheckedAt = latestRecord.CheckedAt,
                            CheckedBy = latestRecord.CheckedBy,
                            ConditionComment = latestRecord.ConditionComment
                        });

                        continue;
                    }

                    if (latestRecord.ActualLocationId.HasValue && latestRecord.ActualLocationId.Value != locationId)
                    {
                        discrepancies.Add(new LocationInventoryDiscrepancyDto
                        {
                            EquipmentId = equipment.Id,
                            InventoryNumber = equipment.InventoryNumber,
                            Name = equipment.Name,
                            EquipmentTypeName = equipment.EquipmentType.Name,
                            EquipmentStatusName = equipment.EquipmentStatus.Name,
                            ExpectedLocationName = equipment.Location.GetDisplayName(),
                            ActualLocationName = latestRecord.ActualLocation?.GetDisplayName(),
                            DiscrepancyCode = "moved-out",
                            DiscrepancyTitle = "Фактически находится в другой локации",
                            DiscrepancySummary = BuildDiscrepancyIssueDescription(
                                equipment.Location.GetDisplayName(),
                                latestRecord.ActualLocation?.GetDisplayName()),
                            CheckedAt = latestRecord.CheckedAt,
                            CheckedBy = latestRecord.CheckedBy,
                            ConditionComment = latestRecord.ConditionComment
                        });
                    }

                    continue;
                }

                if (latestRecord.IsFound && latestRecord.ActualLocationId == locationId)
                {
                    discrepancies.Add(new LocationInventoryDiscrepancyDto
                    {
                        EquipmentId = equipment.Id,
                        InventoryNumber = equipment.InventoryNumber,
                        Name = equipment.Name,
                        EquipmentTypeName = equipment.EquipmentType.Name,
                        EquipmentStatusName = equipment.EquipmentStatus.Name,
                        ExpectedLocationName = equipment.Location.GetDisplayName(),
                        ActualLocationName = latestRecord.ActualLocation?.GetDisplayName(),
                        DiscrepancyCode = "moved-in",
                        DiscrepancyTitle = "Найдено в этой локации с расхождением",
                        DiscrepancySummary = $"По карточке объект закреплен за локацией {equipment.Location.GetDisplayName()}, но на последней инвентаризации найден в текущей локации.",
                        CheckedAt = latestRecord.CheckedAt,
                        CheckedBy = latestRecord.CheckedBy,
                        ConditionComment = latestRecord.ConditionComment
                    });
                }
            }

            return discrepancies
                .OrderBy(item => GetLocationDiscrepancySortOrder(item.DiscrepancyCode))
                .ThenByDescending(item => item.CheckedAt)
                .ThenBy(item => item.InventoryNumber)
                .ToList();
        }

        private static bool RecordRelatesToLocation(int locationId, IReadOnlyList<Equipment> equipmentItems, InventoryRecord record)
        {
            var equipment = equipmentItems.FirstOrDefault(item => item.Id == record.EquipmentId);
            if (equipment is null)
            {
                return false;
            }

            return equipment.LocationId == locationId || record.ActualLocationId == locationId;
        }

        private static string BuildLocationInventoryStatus(Equipment equipment, InventoryRecord? latestRecord)
        {
            if (latestRecord is null)
            {
                return "Инвентаризация еще не подтверждала объект.";
            }

            if (!latestRecord.IsFound)
            {
                return "На последней инвентаризации объект не найден.";
            }

            if (latestRecord.ActualLocationId.HasValue && latestRecord.ActualLocationId.Value != equipment.LocationId)
            {
                var actualLocationName = latestRecord.ActualLocation?.GetDisplayName() ?? "другой локации";
                return $"На последней инвентаризации объект найден в локации {actualLocationName}.";
            }

            return "Последняя инвентаризация подтвердила объект в этой локации.";
        }

        private static int GetLocationDiscrepancySortOrder(string discrepancyCode)
        {
            return discrepancyCode switch
            {
                "missing" => 1,
                "moved-out" => 2,
                "moved-in" => 3,
                _ => 4
            };
        }

        private static bool MatchesWarrantyFilter(DateTime? warrantyEndDate, string? warrantyFilter)
        {
            if (string.IsNullOrWhiteSpace(warrantyFilter))
            {
                return true;
            }

            var daysLeft = GetWarrantyDaysLeft(warrantyEndDate);

            return warrantyFilter switch
            {
                "expired" => daysLeft.HasValue && daysLeft.Value < 0,
                "30" => daysLeft.HasValue && daysLeft.Value >= 0 && daysLeft.Value <= 30,
                "60" => daysLeft.HasValue && daysLeft.Value >= 0 && daysLeft.Value <= 60,
                "90" => daysLeft.HasValue && daysLeft.Value >= 0 && daysLeft.Value <= 90,
                _ => true
            };
        }

        private static int? GetWarrantyDaysLeft(DateTime? warrantyEndDate)
        {
            return warrantyEndDate.HasValue
                ? (warrantyEndDate.Value.Date - DateTime.Today).Days
                : null;
        }

        private static int GetWarrantySortOrder(DateTime? warrantyEndDate)
        {
            var daysLeft = GetWarrantyDaysLeft(warrantyEndDate);
            if (!daysLeft.HasValue)
            {
                return 5;
            }

            if (daysLeft.Value < 0)
            {
                return 1;
            }

            if (daysLeft.Value <= 30)
            {
                return 2;
            }

            if (daysLeft.Value <= 60)
            {
                return 3;
            }

            if (daysLeft.Value <= 90)
            {
                return 4;
            }

            return 5;
        }

        private async Task WriteUpdateHistoryAsync(
            int equipmentId,
            string changedBy,
            string oldInventoryNumber,
            string newInventoryNumber,
            string oldName,
            string newName,
            string? oldSerialNumber,
            string? newSerialNumber,
            string? oldManufacturer,
            string? newManufacturer,
            string? oldModel,
            string? newModel,
            DateTime? oldPurchaseDate,
            DateTime? newPurchaseDate,
            DateTime? oldCommissioningDate,
            DateTime? newCommissioningDate,
            DateTime? oldWarrantyEndDate,
            DateTime? newWarrantyEndDate,
            string? oldResponsiblePerson,
            string? newResponsiblePerson,
            string? oldNotes,
            string? newNotes,
            int oldTypeId,
            int newTypeId,
            int oldStatusId,
            int newStatusId,
            int oldLocationId,
            int newLocationId)
        {
            var historyRecords = new List<HistoryRecordRequest>();

            if (!string.Equals(oldInventoryNumber, newInventoryNumber, StringComparison.Ordinal))
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "InventoryNumber",
                    OldValue: oldInventoryNumber,
                    NewValue: newInventoryNumber,
                    Comment: "Изменен инвентарный номер."));
            }

            if (!string.Equals(oldName, newName, StringComparison.Ordinal))
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "Name",
                    OldValue: oldName,
                    NewValue: newName,
                    Comment: "Изменено наименование оборудования."));
            }

            if (!string.Equals(oldSerialNumber, newSerialNumber, StringComparison.Ordinal))
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "SerialNumber",
                    OldValue: oldSerialNumber,
                    NewValue: newSerialNumber,
                    Comment: "Изменен серийный номер."));
            }

            if (!string.Equals(oldManufacturer, newManufacturer, StringComparison.Ordinal))
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "Manufacturer",
                    OldValue: oldManufacturer,
                    NewValue: newManufacturer,
                    Comment: "Изменен производитель."));
            }

            if (!string.Equals(oldModel, newModel, StringComparison.Ordinal))
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "Model",
                    OldValue: oldModel,
                    NewValue: newModel,
                    Comment: "Изменена модель оборудования."));
            }

            if (oldPurchaseDate != newPurchaseDate)
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "PurchaseDate",
                    OldValue: FormatDate(oldPurchaseDate),
                    NewValue: FormatDate(newPurchaseDate),
                    Comment: "Изменена дата покупки."));
            }

            if (oldCommissioningDate != newCommissioningDate)
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "CommissioningDate",
                    OldValue: FormatDate(oldCommissioningDate),
                    NewValue: FormatDate(newCommissioningDate),
                    Comment: "Изменена дата ввода в эксплуатацию."));
            }

            if (oldWarrantyEndDate != newWarrantyEndDate)
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "WarrantyEndDate",
                    OldValue: FormatDate(oldWarrantyEndDate),
                    NewValue: FormatDate(newWarrantyEndDate),
                    Comment: "Изменена дата окончания гарантии."));
            }

            if (!string.Equals(oldResponsiblePerson, newResponsiblePerson, StringComparison.Ordinal))
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "ResponsiblePerson",
                    OldValue: oldResponsiblePerson,
                    NewValue: newResponsiblePerson,
                    Comment: "Изменено ответственное лицо."));
            }

            if (!string.Equals(oldNotes, newNotes, StringComparison.Ordinal))
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "Notes",
                    OldValue: oldNotes,
                    NewValue: newNotes,
                    Comment: "Изменено примечание."));
            }

            if (oldTypeId != newTypeId)
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.Updated,
                    changedBy,
                    ChangedField: "EquipmentTypeId",
                    OldValue: oldTypeId.ToString(),
                    NewValue: newTypeId.ToString(),
                    Comment: "Изменен тип оборудования."));
            }

            if (oldStatusId != newStatusId)
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.StatusChanged,
                    changedBy,
                    ChangedField: "EquipmentStatusId",
                    OldValue: oldStatusId.ToString(),
                    NewValue: newStatusId.ToString(),
                    Comment: "Изменен статус оборудования при редактировании карточки."));
            }

            if (oldLocationId != newLocationId)
            {
                historyRecords.Add(new HistoryRecordRequest(
                    equipmentId,
                    HistoryActionType.LocationChanged,
                    changedBy,
                    ChangedField: "LocationId",
                    OldValue: oldLocationId.ToString(),
                    NewValue: newLocationId.ToString(),
                    Comment: "Изменено местоположение оборудования при редактировании карточки."));
            }

            await _equipmentHistoryService.AddHistoryRecordsAsync(historyRecords);
        }

        private static string? FormatDate(DateTime? date)
        {
            return date?.ToString("yyyy-MM-dd");
        }
    }
}
