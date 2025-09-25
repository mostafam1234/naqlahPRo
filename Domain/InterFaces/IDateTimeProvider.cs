using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InterFaces
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }

        DateTimeKind Kind { get; }

        bool SupportsMultipleTimeZone { get; }

        DateTime Normalize(DateTime dateTime);
    }
}
