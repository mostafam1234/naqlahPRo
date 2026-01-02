using Application.Features.AdminSection.DeliveryManFeature.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record GetDeliveryManStatisticsQuery : IRequest<Result<DeliveryManStatisticsDto>>
    {
        private class GetDeliveryManStatisticsQueryHandler : IRequestHandler<GetDeliveryManStatisticsQuery, Result<DeliveryManStatisticsDto>>
        {
            private readonly INaqlahContext _context;

            public GetDeliveryManStatisticsQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<DeliveryManStatisticsDto>> Handle(GetDeliveryManStatisticsQuery request, CancellationToken cancellationToken)
            {
                var totalDeliveryMen = await _context.DeliveryMen
                    .Where(dm => dm.DeliveryState == DeliveryRequesState.Approved)
                    .CountAsync(cancellationToken);

                var activeDeliveryMen = await _context.DeliveryMen
                    .Where(dm => dm.DeliveryState == DeliveryRequesState.Approved && dm.Active)
                    .CountAsync(cancellationToken);

                var inactiveDeliveryMen = totalDeliveryMen - activeDeliveryMen;

                var result = new DeliveryManStatisticsDto
                {
                    TotalDeliveryMen = totalDeliveryMen,
                    ActiveDeliveryMen = activeDeliveryMen,
                    InactiveDeliveryMen = inactiveDeliveryMen
                };

                return Result.Success(result);
            }
        }
    }
}

