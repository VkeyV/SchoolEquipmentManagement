using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class EquipmentHistoryItemDto
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? ChangedField { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Comment { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}
