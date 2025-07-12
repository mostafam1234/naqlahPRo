using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DomainEventsHelper
{
    public interface IEntity
    {
        void AddEvent(Event @event);
        void ClearEvents();
        IReadOnlyList<Event> Events { get; }
        void clearEventById(string eventId);
    }
}
