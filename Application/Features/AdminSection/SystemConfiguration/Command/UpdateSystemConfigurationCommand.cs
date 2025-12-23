using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.SystemConfiguration.Command
{
    public sealed record UpdateSystemConfigurationCommand:IRequest<Result>
    {
        public int Id { get; set; }
        public int BaseKm { get; set; }
        public decimal BaseKmRate { get; set; }
        public decimal ExtraKmRate { get; set; }
        public int BaseHours { get; set; }
        public decimal BaseHourRate { get; set; }
        public decimal ExtraHourRate { get; set; }
        public decimal VatRate { get; set; }
        public decimal ServiceFess { get; set; }


        private class UpdateSystemConfigurationCommandHandler : IRequestHandler<UpdateSystemConfigurationCommand, Result>
        {
            private readonly INaqlahContext naqlahContext;
            public UpdateSystemConfigurationCommandHandler(INaqlahContext naqlahContext)
            {
                this.naqlahContext = naqlahContext;
            }
            public async Task<Result> Handle(UpdateSystemConfigurationCommand request, CancellationToken cancellationToken)
            {
                var config = await naqlahContext.SystemConfigurations.AsTracking().FirstOrDefaultAsync(x=>x.Id==request.Id);
                if (config == null)
                {
                    return Result.Failure("Configuration not found");
                }
                config.Update(request.BaseKm,
                              request.BaseKmRate,
                              request.ExtraKmRate,
                              request.BaseHours,
                              request.BaseHourRate,
                              request.ExtraHourRate,
                              request.VatRate,
                              request.ServiceFess);


               var saveResult= await naqlahContext.SaveChangesAsyncWithResult();
                return Result.Success();
            }
        }
    }
}
