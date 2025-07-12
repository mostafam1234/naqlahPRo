using Domain.DomainEventsHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InterFaces
{
    public interface IHangfireJobWriter
    {
        void EnqueueBackGroundJob(Event job);
    }
}
