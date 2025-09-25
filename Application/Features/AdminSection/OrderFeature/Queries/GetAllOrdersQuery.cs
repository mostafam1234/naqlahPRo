using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Features.VehicleSection.Dtos;
using Application.Features.VehicleSection.Queries;
using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Queries
{
    public class GetAllOrdersQuery:IRequest<Result<PagedResult<GetAllOrdersDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        private class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<PagedResult<GetAllOrdersDto>>>
        {
            public GetAllOrdersQueryHandler()
            {

            }

            public async Task<Result<PagedResult<GetAllOrdersDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
            {
                var pagedResult = new PagedResult<GetAllOrdersDto>
                {

                };

                return Result.Success(pagedResult);
            }
        }       

    }
}
