using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class EquipmentFilterDto
    {
        public string? Search { get; set; }
        public int? TypeId { get; set; }
        public int? StatusId { get; set; }
        public int? LocationId { get; set; }
        public string? WarrantyFilter { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
