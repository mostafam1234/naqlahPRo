using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DomainEventsHelper
{
    public abstract record Event : INotification
    {
        public string EventId { get; init; } = Guid.NewGuid().ToString();
    }
}
