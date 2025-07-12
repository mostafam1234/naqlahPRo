using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DomainEventsHelper
{
    public abstract class Entity : IEntity
    {
        private List<Event> events = new List<Event>();
        public void AddEvent(Event @event)
        {
            if (@event != null)
            {
                events.Add(@event);
            }
        }

        public void ClearEvents()
        {
            events.Clear();
        }


        public IReadOnlyList<Event> Events => events.ToList();


        public void clearEventById(string eventId)
        {
            var eventToRemove = events.FirstOrDefault(x => x.EventId.Equals(eventId));
            if(eventToRemove is null)
            {
                return;
            }
            events.Remove(eventToRemove);
        }
    }
}
