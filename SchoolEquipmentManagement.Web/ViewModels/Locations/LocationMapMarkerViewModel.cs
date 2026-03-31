using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.ViewModels.Locations
{
    public class LocationMapMarkerViewModel
    {
        public string Id => $"marker-{EquipmentId}";
        public int EquipmentId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ResponsiblePerson { get; set; }
        public string ZoneCode { get; set; } = string.Empty;
        public double LeftPercent { get; set; }
        public double TopPercent { get; set; }
        public string MarkerClass { get; set; } = "is-active";
        public string StateLabel { get; set; } = string.Empty;
        public string ShortLabel { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? DiscrepancyTitle { get; set; }
        public string? ExpectedLocation { get; set; }
        public string? ActualLocation { get; set; }
        public bool IsDiscrepancy { get; set; }
        public bool CanChangeStatus { get; set; }
        public bool CanAssignResponsible { get; set; }

        public string DisplayResponsiblePerson => EquipmentDisplayFormatter.Text(ResponsiblePerson);
        public string DisplayExpectedLocation => EquipmentDisplayFormatter.Text(ExpectedLocation);
        public string DisplayActualLocation => EquipmentDisplayFormatter.Text(ActualLocation);
        public string InlineStyle => $"left:{LeftPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}%;top:{TopPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}%;";
    }
}
