using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum DeliveryRequesState
    {
        New = 1,
        Approved = 2,
        Rejected = 3,
        Blocked = 4,
        Suspended = 5
    }
}
