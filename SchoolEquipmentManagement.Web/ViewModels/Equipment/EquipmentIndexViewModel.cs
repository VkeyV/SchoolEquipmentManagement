using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolEquipmentManagement.Web.ViewModels.Equipment
{
    public class EquipmentIndexViewModel
    {
        public List<EquipmentListViewModel> Items { get; set; } = new();
        public string? Search { get; set; }
        public int? TypeId { get; set; }
        public int? StatusId { get; set; }
        public int? LocationId { get; set; }
        public string? WarrantyFilter { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<SelectListItem> EquipmentTypes { get; set; } = new();
        public List<SelectListItem> EquipmentStatuses { get; set; } = new();
        public List<SelectListItem> Locations { get; set; } = new();
        public List<SelectListItem> WarrantyFilters { get; set; } = new();
        public bool CanCreate { get; set; }
        public bool CanImport { get; set; }

        public bool HasFilters =>
            !string.IsNullOrWhiteSpace(Search) ||
            TypeId.HasValue ||
            StatusId.HasValue ||
            LocationId.HasValue ||
            !string.IsNullOrWhiteSpace(WarrantyFilter);

        public int StartItem => PaginationDisplayHelper.GetStartItem(Page, PageSize, TotalCount);
        public int EndItem => PaginationDisplayHelper.GetEndItem(Page, PageSize, TotalCount);
        public bool IsFirstPage => Page <= 1;
        public bool IsLastPage => Page >= TotalPages;
        public int PreviousPage => IsFirstPage ? 1 : Page - 1;
        public int NextPage => IsLastPage ? TotalPages : Page + 1;
    }
}
