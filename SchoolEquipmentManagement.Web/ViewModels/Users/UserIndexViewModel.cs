namespace SchoolEquipmentManagement.Web.ViewModels.Users
{
    public class UserIndexViewModel
    {
        public List<UserListItemViewModel> Items { get; set; } = new();
        public int TotalCount => Items.Count;
        public int ActiveCount => Items.Count(x => x.IsActive);
        public int InactiveCount => Items.Count(x => !x.IsActive);
        public int LockedCount => Items.Count(x => x.IsLockedOut);
    }
}
