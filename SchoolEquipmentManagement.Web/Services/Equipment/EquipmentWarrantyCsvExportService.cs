using System.Globalization;
using System.Text;
using SchoolEquipmentManagement.Application.DTOs;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public sealed class EquipmentWarrantyCsvExportService : IEquipmentWarrantyCsvExportService
{
    private static readonly string[] Headers =
    [
        "InventoryNumber",
        "Name",
        "EquipmentType",
        "Status",
        "Location",
        "SerialNumber",
        "Manufacturer",
        "Model",
        "PurchaseDate",
        "CommissioningDate",
        "WarrantyEndDate",
        "ResponsiblePerson",
        "Notes",
        "Id",
        "WarrantyDaysLeft",
        "WarrantyRiskCode",
        "HistoryCount",
        "LastHistoryEvent",
        "LastHistoryAt",
        "DetailsUrl"
    ];

    public byte[] Export(IReadOnlyList<EquipmentDetailsDto> items, Func<int, string> detailsUrlFactory)
    {
        var builder = new StringBuilder();
        AppendRow(builder, Headers);

        foreach (var item in items)
        {
            var orderedHistory = item.History
                .OrderByDescending(entry => entry.ChangedAt)
                .ThenByDescending(entry => entry.Id)
                .ToList();

            var lastHistory = orderedHistory.FirstOrDefault();
            var warrantyDaysLeft = GetWarrantyDaysLeft(item.WarrantyEndDate);

            AppendRow(builder, new string?[]
            {
                item.InventoryNumber,
                item.Name,
                item.EquipmentTypeName,
                item.EquipmentStatusName,
                item.LocationName,
                item.SerialNumber,
                item.Manufacturer,
                item.Model,
                FormatDate(item.PurchaseDate),
                FormatDate(item.CommissioningDate),
                FormatDate(item.WarrantyEndDate),
                item.ResponsiblePerson,
                item.Notes,
                item.Id.ToString(CultureInfo.InvariantCulture),
                warrantyDaysLeft?.ToString(CultureInfo.InvariantCulture),
                GetWarrantyRiskCode(warrantyDaysLeft),
                orderedHistory.Count.ToString(CultureInfo.InvariantCulture),
                lastHistory is null ? string.Empty : $"{lastHistory.ActionType} / {lastHistory.ChangedBy}",
                lastHistory is null ? string.Empty : FormatDateTime(lastHistory.ChangedAt),
                detailsUrlFactory(item.Id)
            });
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(builder.ToString());
    }

    private static string FormatDate(DateTime? value) =>
        value.HasValue
            ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : string.Empty;

    private static string FormatDateTime(DateTime value) =>
        value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    private static int? GetWarrantyDaysLeft(DateTime? warrantyEndDate) =>
        warrantyEndDate.HasValue
            ? (warrantyEndDate.Value.Date - DateTime.Today).Days
            : null;

    private static string GetWarrantyRiskCode(int? warrantyDaysLeft)
    {
        if (!warrantyDaysLeft.HasValue)
        {
            return "unknown";
        }

        if (warrantyDaysLeft.Value < 0)
        {
            return "expired";
        }

        if (warrantyDaysLeft.Value <= 30)
        {
            return "high";
        }

        if (warrantyDaysLeft.Value <= 60)
        {
            return "medium";
        }

        if (warrantyDaysLeft.Value <= 90)
        {
            return "low";
        }

        return "planned";
    }

    private static void AppendRow(StringBuilder builder, IEnumerable<string?> values)
    {
        builder.AppendJoin(';', values.Select(Escape));
        builder.AppendLine();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "\"\"";
        }

        var normalizedValue = value
            .Replace("\r\n", " / ", StringComparison.Ordinal)
            .Replace("\n", " / ", StringComparison.Ordinal)
            .Replace("\r", " / ", StringComparison.Ordinal)
            .Replace("\"", "'", StringComparison.Ordinal)
            .Trim();

        return $"\"{normalizedValue}\"";
    }
}
