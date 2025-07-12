using Domain.DomainEventsHelper;
using Domain.InterFaces;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HangFireDepencies
{
    public class HangfireJobWriter: IHangfireJobWriter
    {
        private readonly HangfireBridge hangFireBridge;
        private readonly IBackgroundJobClient backgroundJobClient;

        public HangfireJobWriter(HangfireBridge hangFireBridge,
                                 IBackgroundJobClient backgroundJobClient)
        {
            this.hangFireBridge = hangFireBridge;
            this.backgroundJobClient = backgroundJobClient;
        }


        public void EnqueueBackGroundJob(Event job)
        {
            backgroundJobClient.Enqueue(() =>hangFireBridge.Puplish(job));
        }
    }
}
