using Application.Features.AdminSection.OrderFeature.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Queries
{
    public sealed record GetAvailableDeliveryMenForAssignmentQuery(int LanguageId = 1) : IRequest<Result<List<AvailableDeliveryManDto>>>
    {
        private class GetAvailableDeliveryMenForAssignmentQueryHandler : IRequestHandler<GetAvailableDeliveryMenForAssignmentQuery, Result<List<AvailableDeliveryManDto>>>
        {
            private readonly INaqlahContext _context;

            public GetAvailableDeliveryMenForAssignmentQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<List<AvailableDeliveryManDto>>> Handle(GetAvailableDeliveryMenForAssignmentQuery request, CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;

                // Get approved and active delivery men who don't have active orders (Assigned status)
                var deliveryMenWithActiveOrders = await _context.Orders
                    .Where(o => o.OrderStatus == OrderStatus.Assigned && o.DeliveryManId.HasValue)
                    .Select(o => o.DeliveryManId!.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var availableDeliveryMen = await _context.DeliveryMen
                    .Include(dm => dm.User)
                    .Include(dm => dm.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Where(dm => dm.DeliveryState == DeliveryRequesState.Approved 
                              && dm.Active 
                              && !deliveryMenWithActiveOrders.Contains(dm.Id)
                              && dm.Vehicle != null)
                    .Select(dm => new AvailableDeliveryManDto
                    {
                        DeliveryManId = dm.Id,
                        FullName = dm.FullName,
                        PhoneNumber = dm.PhoneNumber,
                        VehicleTypeName = isArabic ? dm.Vehicle!.VehicleType!.ArabicName : dm.Vehicle!.VehicleType!.EnglishName,
                        VehiclePlate = dm.Vehicle!.LicensePlateNumber ?? string.Empty
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(availableDeliveryMen);
            }
        }
    }
}

