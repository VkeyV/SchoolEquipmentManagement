using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class ChangeEquipmentStatusDto
    {
        public int EquipmentId { get; set; }
        public int NewStatusId { get; set; }
        public string ChangedBy { get; set; } = "System";
        public string? Comment { get; set; }
    }
}
