using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolEquipmentManagement.Domain.Common
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }

        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
