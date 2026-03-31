using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventorySessionRepository _inventorySessionRepository;
        private readonly IInventoryRecordRepository _inventoryRecordRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IEquipmentHistoryService _equipmentHistoryService;

        public InventoryService(
            IInventorySessionRepository inventorySessionRepository,
            IInventoryRecordRepository inventoryRecordRepository,
            IEquipmentRepository equipmentRepository,
            IEquipmentHistoryService equipmentHistoryService)
        {
            _inventorySessionRepository = inventorySessionRepository;
            _inventoryRecordRepository = inventoryRecordRepository;
            _equipmentRepository = equipmentRepository;
            _equipmentHistoryService = equipmentHistoryService;
        }

        public async Task<List<InventorySessionListItemDto>> GetSessionsAsync()
        {
            var sessions = await _inventorySessionRepository.GetAllAsync();

            return sessions.Select(session =>
            {
                var summary = BuildSummary(session);

                return new InventorySessionListItemDto
                {
                    Id = session.Id,
                    Name = session.Name,
                    StartDate = session.StartDate,
                    EndDate = session.EndDate,
                    Status = GetSessionStatusDisplayName(session.Status),
                    CreatedBy = session.CreatedBy,
                    CheckedCount = summary.CheckedCount,
                    FoundCount = summary.FoundCount,
                    MissingCount = summary.MissingCount,
                    DiscrepancyCount = summary.DiscrepancyCount
                };
            }).ToList();
        }

        public async Task<InventorySessionDetailsDto?> GetSessionDetailsAsync(int id)
        {
            var session = await _inventorySessionRepository.GetByIdAsync(id);
            if (session is null)
            {
                return null;
            }

            var equipmentItems = await _equipmentRepository.GetAllAsync();
            var recordsByEquipmentId = session.Records.ToDictionary(x => x.EquipmentId);
            var summary = BuildSummary(session);

            return new InventorySessionDetailsDto
            {
                Id = session.Id,
                Name = session.Name,
                StartDate = session.StartDate,
                EndDate = session.EndDate,
                Status = GetSessionStatusDisplayName(session.Status),
                CreatedBy = session.CreatedBy,
                TotalEquipmentCount = equipmentItems.Count,
                CheckedCount = summary.CheckedCount,
                FoundCount = summary.FoundCount,
                MissingCount = summary.MissingCount,
                DiscrepancyCount = summary.DiscrepancyCount,
                EquipmentItems = equipmentItems.Select(equipment =>
                {
                    recordsByEquipmentId.TryGetValue(equipment.Id, out var record);

                    return new InventorySessionEquipmentItemDto
                    {
                        EquipmentId = equipment.Id,
                        InventoryNumber = equipment.InventoryNumber,
                        Name = equipment.Name,
                        EquipmentTypeName = equipment.EquipmentType.Name,
                        ExpectedLocationName = equipment.Location.GetDisplayName(),
                        EquipmentStatusName = equipment.EquipmentStatus.Name,
                        IsChecked = record is not null,
                        IsFound = record?.IsFound,
                        ActualLocationId = record?.ActualLocationId,
                        ActualLocationName = record?.ActualLocation?.GetDisplayName(),
                        ConditionComment = record?.ConditionComment,
                        CheckedAt = record?.CheckedAt,
                        CheckedBy = record?.CheckedBy,
                        HasLocationDiscrepancy = record is not null &&
                            record.IsFound &&
                            record.ActualLocationId.HasValue &&
                            record.ActualLocationId != equipment.LocationId
                    };
                }).OrderBy(x => x.InventoryNumber).ToList()
            };
        }

        public async Task<int> CreateSessionAsync(CreateInventorySessionDto dto)
        {
            ValidateAuditUser(dto.CreatedBy);

            var session = new InventorySession(dto.Name, dto.StartDate, dto.CreatedBy);
            await _inventorySessionRepository.AddAsync(session);
            await _inventorySessionRepository.SaveChangesAsync();

            return session.Id;
        }

        public async Task StartSessionAsync(int id)
        {
            var session = await _inventorySessionRepository.GetByIdAsync(id)
                ?? throw new DomainException("Сессия инвентаризации не найдена.");

            session.Start();
            await _inventorySessionRepository.UpdateAsync(session);
            await _inventorySessionRepository.SaveChangesAsync();
        }

        public async Task CompleteSessionAsync(int id)
        {
            var session = await _inventorySessionRepository.GetByIdAsync(id)
                ?? throw new DomainException("Сессия инвентаризации не найдена.");

            session.Complete(DateTime.UtcNow);
            await _inventorySessionRepository.UpdateAsync(session);
            await _inventorySessionRepository.SaveChangesAsync();
        }

        public async Task<InventorySessionEquipmentItemDto?> GetCheckItemAsync(int sessionId, int equipmentId)
        {
            var details = await GetSessionDetailsAsync(sessionId);
            return details?.EquipmentItems.FirstOrDefault(x => x.EquipmentId == equipmentId);
        }

        public async Task RecordCheckAsync(InventoryCheckDto dto)
        {
            ValidateAuditUser(dto.CheckedBy);

            var session = await _inventorySessionRepository.GetByIdAsync(dto.SessionId)
                ?? throw new DomainException("Сессия инвентаризации не найдена.");

            if (session.Status != InventorySessionStatus.InProgress)
            {
                throw new DomainException("Фиксировать результаты можно только для активной инвентаризации.");
            }

            var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId)
                ?? throw new DomainException("Оборудование не найдено.");

            var actualLocationId = dto.IsFound
                ? dto.ActualLocationId ?? equipment.LocationId
                : dto.ActualLocationId;

            var existingRecord = await _inventoryRecordRepository.GetBySessionAndEquipmentAsync(dto.SessionId, dto.EquipmentId);
            var newResultDescription = BuildCheckResultDescription(dto.IsFound, equipment.LocationId, actualLocationId);

            if (existingRecord is null)
            {
                var record = new InventoryRecord(
                    dto.SessionId,
                    dto.EquipmentId,
                    dto.IsFound,
                    dto.CheckedBy,
                    actualLocationId,
                    dto.ConditionComment);

                await _inventoryRecordRepository.AddAsync(record);
                await _inventoryRecordRepository.SaveChangesAsync();

                await _equipmentHistoryService.AddHistoryRecordAsync(
                    equipment.Id,
                    HistoryActionType.InventoryChecked,
                    dto.CheckedBy,
                    changedField: "InventoryCheck",
                    oldValue: "Не проверено",
                    newValue: newResultDescription,
                    comment: BuildInventoryComment(session.Name, dto.ConditionComment));

                return;
            }

            var oldResultDescription = BuildCheckResultDescription(
                existingRecord.IsFound,
                equipment.LocationId,
                existingRecord.ActualLocationId);

            existingRecord.UpdateResult(dto.IsFound, actualLocationId, dto.ConditionComment);
            await _inventoryRecordRepository.UpdateAsync(existingRecord);
            await _inventoryRecordRepository.SaveChangesAsync();

            await _equipmentHistoryService.AddHistoryRecordAsync(
                equipment.Id,
                HistoryActionType.InventoryChecked,
                dto.CheckedBy,
                changedField: "InventoryCheck",
                oldValue: oldResultDescription,
                newValue: newResultDescription,
                comment: BuildInventoryComment(session.Name, dto.ConditionComment));
        }

        private static void ValidateAuditUser(string changedBy)
        {
            if (string.IsNullOrWhiteSpace(changedBy))
            {
                throw new DomainException("Не указан пользователь, выполнивший операцию.");
            }
        }

        private static string BuildInventoryComment(string sessionName, string? conditionComment)
        {
            if (string.IsNullOrWhiteSpace(conditionComment))
            {
                return $"Инвентаризация: {sessionName}.";
            }

            return $"Инвентаризация: {sessionName}. {conditionComment.Trim()}";
        }

        private static string BuildCheckResultDescription(bool isFound, int expectedLocationId, int? actualLocationId)
        {
            if (!isFound)
            {
                return "Не найдено";
            }

            if (actualLocationId.HasValue && actualLocationId.Value != expectedLocationId)
            {
                return "Найдено с расхождением по местоположению";
            }

            return "Найдено";
        }

        private static (int CheckedCount, int FoundCount, int MissingCount, int DiscrepancyCount) BuildSummary(InventorySession session)
        {
            var checkedCount = session.Records.Count;
            var foundCount = session.Records.Count(x => x.IsFound);
            var missingCount = session.Records.Count(x => !x.IsFound);
            var discrepancyCount = session.Records.Count(x =>
                x.IsFound &&
                x.ActualLocationId.HasValue &&
                x.ActualLocationId != x.Equipment.LocationId);

            return (checkedCount, foundCount, missingCount, discrepancyCount);
        }

        private static string GetSessionStatusDisplayName(InventorySessionStatus status)
        {
            return status switch
            {
                InventorySessionStatus.Draft => "Черновик",
                InventorySessionStatus.InProgress => "Активна",
                InventorySessionStatus.Completed => "Завершена",
                InventorySessionStatus.Cancelled => "Отменена",
                _ => status.ToString()
            };
        }
    }
}
