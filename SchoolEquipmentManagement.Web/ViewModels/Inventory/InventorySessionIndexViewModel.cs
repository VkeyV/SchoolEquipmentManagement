namespace SchoolEquipmentManagement.Web.ViewModels.Inventory
{
    public class InventorySessionIndexViewModel
    {
        public List<InventorySessionListItemViewModel> Sessions { get; set; } = new();
        public bool CanCreateSession { get; set; }
    }
}
