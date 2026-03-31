using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Domain.Exceptions;
using System.Globalization;
using System.Text;

namespace SchoolEquipmentManagement.Application.Services
{
    public class EquipmentImportService : IEquipmentImportService
    {
        private static readonly string[] RequiredHeaders =
        {
            "inventorynumber",
            "name",
            "equipmenttype",
            "status",
            "location"
        };

        private readonly IDictionaryService _dictionaryService;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IEquipmentService _equipmentService;

        public EquipmentImportService(
            IDictionaryService dictionaryService,
            IEquipmentRepository equipmentRepository,
            IEquipmentService equipmentService)
        {
            _dictionaryService = dictionaryService;
            _equipmentRepository = equipmentRepository;
            _equipmentService = equipmentService;
        }

        public async Task<EquipmentImportPreviewDto> PreviewCsvAsync(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var content = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new DomainException("Файл импорта пустой.");
            }

            var lines = content
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (lines.Count < 2)
            {
                throw new DomainException("CSV-файл должен содержать заголовок и хотя бы одну строку данных.");
            }

            var headers = ParseCsvLine(lines[0]);
            var normalizedHeaders = headers.Select(NormalizeHeader).ToList();

            var missingHeaders = RequiredHeaders
                .Where(required => !normalizedHeaders.Contains(required))
                .ToList();

            if (missingHeaders.Count > 0)
            {
                throw new DomainException(
                    $"В файле отсутствуют обязательные колонки: {string.Join(", ", missingHeaders)}.");
            }

            var headerIndex = normalizedHeaders
                .Select((value, index) => new { value, index })
                .ToDictionary(x => x.value, x => x.index);

            var equipmentTypes = await _dictionaryService.GetEquipmentTypesAsync();
            var statuses = await _dictionaryService.GetEquipmentStatusesAsync();
            var locations = await _dictionaryService.GetLocationsAsync();
            var existingInventoryNumbers = (await _equipmentRepository.GetAllAsync())
                .Select(x => x.InventoryNumber)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var typeMap = equipmentTypes.ToDictionary(x => x.Name, x => x.Id, StringComparer.OrdinalIgnoreCase);
            var statusMap = statuses.ToDictionary(x => x.Name, x => x.Id, StringComparer.OrdinalIgnoreCase);
            var locationMap = locations.ToDictionary(x => x.Name, x => x.Id, StringComparer.OrdinalIgnoreCase);
            var fileInventoryNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var preview = new EquipmentImportPreviewDto();

            for (var i = 1; i < lines.Count; i++)
            {
                var rowNumber = i + 1;
                var columns = ParseCsvLine(lines[i]);

                var inventoryNumber = GetColumnValue(columns, headerIndex, "inventorynumber");
                var name = GetColumnValue(columns, headerIndex, "name");
                var equipmentTypeName = GetColumnValue(columns, headerIndex, "equipmenttype");
                var statusName = GetColumnValue(columns, headerIndex, "status");
                var locationName = GetColumnValue(columns, headerIndex, "location");

                var previewRow = new EquipmentImportPreviewRowDto
                {
                    RowNumber = rowNumber,
                    InventoryNumber = inventoryNumber,
                    Name = name,
                    EquipmentTypeName = equipmentTypeName,
                    StatusName = statusName,
                    LocationName = locationName
                };

                ValidateRequired(previewRow.Errors, inventoryNumber, "Инвентарный номер");
                ValidateRequired(previewRow.Errors, name, "Наименование");
                ValidateRequired(previewRow.Errors, equipmentTypeName, "Тип оборудования");
                ValidateRequired(previewRow.Errors, statusName, "Статус");
                ValidateRequired(previewRow.Errors, locationName, "Местоположение");

                if (!string.IsNullOrWhiteSpace(inventoryNumber))
                {
                    if (!fileInventoryNumbers.Add(inventoryNumber))
                    {
                        previewRow.Errors.Add("Инвентарный номер дублируется в импортируемом файле.");
                    }

                    if (existingInventoryNumbers.Contains(inventoryNumber))
                    {
                        previewRow.Errors.Add("Оборудование с таким инвентарным номером уже существует в системе.");
                    }
                }

                if (!TryResolve(typeMap, equipmentTypeName, out var equipmentTypeId))
                {
                    previewRow.Errors.Add("Указан неизвестный тип оборудования.");
                }

                if (!TryResolve(statusMap, statusName, out var statusId))
                {
                    previewRow.Errors.Add("Указан неизвестный статус.");
                }

                if (!TryResolve(locationMap, locationName, out var locationId))
                {
                    previewRow.Errors.Add("Указано неизвестное местоположение.");
                }

                var purchaseDate = ParseDate(previewRow.Errors, GetColumnValue(columns, headerIndex, "purchasedate"), "Дата покупки");
                var commissioningDate = ParseDate(previewRow.Errors, GetColumnValue(columns, headerIndex, "commissioningdate"), "Дата ввода в эксплуатацию");
                var warrantyEndDate = ParseDate(previewRow.Errors, GetColumnValue(columns, headerIndex, "warrantyenddate"), "Дата окончания гарантии");

                if (purchaseDate.HasValue && commissioningDate.HasValue &&
                    commissioningDate.Value.Date < purchaseDate.Value.Date)
                {
                    previewRow.Errors.Add("Дата ввода в эксплуатацию не может быть раньше даты покупки.");
                }

                if (commissioningDate.HasValue && warrantyEndDate.HasValue &&
                    warrantyEndDate.Value.Date < commissioningDate.Value.Date)
                {
                    previewRow.Errors.Add("Дата окончания гарантии не может быть раньше даты ввода в эксплуатацию.");
                }

                preview.Rows.Add(previewRow);

                if (previewRow.IsValid)
                {
                    preview.ValidItems.Add(new EquipmentImportApplyItemDto
                    {
                        InventoryNumber = inventoryNumber,
                        Name = name,
                        EquipmentTypeId = equipmentTypeId,
                        EquipmentStatusId = statusId,
                        LocationId = locationId,
                        SerialNumber = NullIfWhiteSpace(GetColumnValue(columns, headerIndex, "serialnumber")),
                        Manufacturer = NullIfWhiteSpace(GetColumnValue(columns, headerIndex, "manufacturer")),
                        Model = NullIfWhiteSpace(GetColumnValue(columns, headerIndex, "model")),
                        PurchaseDate = purchaseDate,
                        CommissioningDate = commissioningDate,
                        WarrantyEndDate = warrantyEndDate,
                        ResponsiblePerson = NullIfWhiteSpace(GetColumnValue(columns, headerIndex, "responsibleperson")),
                        Notes = NullIfWhiteSpace(GetColumnValue(columns, headerIndex, "notes"))
                    });
                }
            }

            return preview;
        }

        public async Task<EquipmentImportResultDto> ImportAsync(IEnumerable<EquipmentImportApplyItemDto> items, string changedBy)
        {
            var importItems = items.ToList();
            if (importItems.Count == 0)
            {
                throw new DomainException("Нет корректных строк для импорта.");
            }

            foreach (var item in importItems)
            {
                await _equipmentService.CreateEquipmentAsync(new CreateEquipmentDto
                {
                    InventoryNumber = item.InventoryNumber,
                    Name = item.Name,
                    EquipmentTypeId = item.EquipmentTypeId,
                    EquipmentStatusId = item.EquipmentStatusId,
                    LocationId = item.LocationId,
                    SerialNumber = item.SerialNumber,
                    Manufacturer = item.Manufacturer,
                    Model = item.Model,
                    PurchaseDate = item.PurchaseDate,
                    CommissioningDate = item.CommissioningDate,
                    WarrantyEndDate = item.WarrantyEndDate,
                    ResponsiblePerson = item.ResponsiblePerson,
                    Notes = item.Notes,
                    ChangedBy = changedBy
                });
            }

            return new EquipmentImportResultDto
            {
                ImportedCount = importItems.Count
            };
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            foreach (var ch in line)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (ch == ';' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            result.Add(current.ToString().Trim());
            return result;
        }

        private static string NormalizeHeader(string header)
        {
            return header
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", string.Empty);
        }

        private static string GetColumnValue(IReadOnlyList<string> columns, IReadOnlyDictionary<string, int> headerIndex, string header)
        {
            return headerIndex.TryGetValue(header, out var index) && index < columns.Count
                ? columns[index].Trim()
                : string.Empty;
        }

        private static void ValidateRequired(List<string> errors, string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Поле «{fieldName}» обязательно для заполнения.");
            }
        }

        private static bool TryResolve(Dictionary<string, int> map, string name, out int id)
        {
            if (!string.IsNullOrWhiteSpace(name) && map.TryGetValue(name.Trim(), out id))
            {
                return true;
            }

            id = 0;
            return false;
        }

        private static DateTime? ParseDate(List<string> errors, string rawValue, string displayName)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            if (DateTime.TryParse(rawValue, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out var ruDate))
            {
                return ruDate.Date;
            }

            if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var invariantDate))
            {
                return invariantDate.Date;
            }

            errors.Add($"Поле «{displayName}» содержит некорректную дату.");
            return null;
        }

        private static string? NullIfWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
