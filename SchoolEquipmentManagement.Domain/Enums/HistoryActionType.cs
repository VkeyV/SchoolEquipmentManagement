using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolEquipmentManagement.Domain.Enums
{
    public enum HistoryActionType
    {
        Created = 1,
        Updated = 2,
        StatusChanged = 3,
        LocationChanged = 4,
        InventoryChecked = 5,
        WrittenOff = 6
    }
}
