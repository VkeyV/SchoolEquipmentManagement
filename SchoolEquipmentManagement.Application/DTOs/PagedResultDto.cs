namespace SchoolEquipmentManagement.Application.DTOs
{
    public class PagedResultDto<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => TotalCount == 0
            ? 1
            : (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
