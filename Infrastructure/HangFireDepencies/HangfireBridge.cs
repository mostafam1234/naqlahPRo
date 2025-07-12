using Domain.DomainEventsHelper;
using Domain.InterFaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HangFireDepencies
{
    public class HangfireBridge
    {
        private readonly IMediator mediator;

        public HangfireBridge(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task Puplish(Event job)
        {
            try
            {
                await mediator.Publish(job);
            }
            catch (Exception EX)
            {


            }

        }
    }
}
