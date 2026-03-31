namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    internal static class PaginationDisplayHelper
    {
        public static int GetStartItem(int page, int pageSize, int totalCount) =>
            totalCount == 0 ? 0 : ((page - 1) * pageSize) + 1;

        public static int GetEndItem(int page, int pageSize, int totalCount) =>
            totalCount == 0 ? 0 : Math.Min(page * pageSize, totalCount);
    }
}
