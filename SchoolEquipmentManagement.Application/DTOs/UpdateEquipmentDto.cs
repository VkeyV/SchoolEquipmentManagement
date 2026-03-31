using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class UpdateEquipmentDto
    {
        public int Id { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int EquipmentTypeId { get; set; }
        public int EquipmentStatusId { get; set; }
        public int LocationId { get; set; }
        public string? SerialNumber { get; set; }
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? CommissioningDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public string? ResponsiblePerson { get; set; }
        public string? Notes { get; set; }
        public string ChangedBy { get; set; } = "System";
    }
}
