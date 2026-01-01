using Application.Features.AdminSection.SystemConfiguration.Dto;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.SystemConfiguration.Query
{
    public sealed record GetSystemConfigurationQuery:IRequest<Result<SystemConfigurationDto>>
    {
        private class GetSystemConfigurationHandler : IRequestHandler<GetSystemConfigurationQuery, Result<SystemConfigurationDto>>
        {
            private readonly INaqlahContext naqlahContext;

            public GetSystemConfigurationHandler(INaqlahContext naqlahContext)
            {
                this.naqlahContext = naqlahContext;
            }
            public async Task<Result<SystemConfigurationDto>> Handle(GetSystemConfigurationQuery request, CancellationToken cancellationToken)
            {
                var config = await naqlahContext.SystemConfigurations.Select(x=>new SystemConfigurationDto
                {
                    Id = x.Id,
                    BaseHourRate = x.BaseHourRate,
                    BaseHours = x.BaseHours,
                    BaseKm = x.BaseKm,
                    BaseKmRate = x.BaseKmRate,
                    ExtraHourRate = x.ExtraHourRate,
                    ExtraKmRate = x.ExtraKmRate,
                    ServiceFess = x.ServiceFess,
                    VatRate = x.VatRate
                }).FirstOrDefaultAsync();

                return config;
            }
        }
    }
}
