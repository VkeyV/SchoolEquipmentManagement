namespace SchoolEquipmentManagement.Web.ViewModels.Locations
{
    public class LocationMapZoneViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public double LeftPercent { get; set; }
        public double TopPercent { get; set; }
        public double WidthPercent { get; set; }
        public double HeightPercent { get; set; }
        public string AccentClass { get; set; } = string.Empty;

        public string InlineStyle =>
            $"left:{LeftPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}%;" +
            $"top:{TopPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}%;" +
            $"width:{WidthPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}%;" +
            $"height:{HeightPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}%;";
    }
}
