using Application.Features.AdminSection.AuditFeature.Dtos;
using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.AuditFeature.Queries
{
    public sealed record GetAuditFilterOptionsQuery : IRequest<Result<AuditFilterOptionsDto>>
    {
        /// <summary>
        /// All entity types (table/model names) that can appear in audit logs.
        /// Must match entry.Entity.GetType().Name in NaqlahContext SaveChanges.
        /// </summary>
        private static readonly string[] AllAuditableEntityTypes =
        {
            "Assistant",
            "AssistanWork",
            "City",
            "Complain",
            "Company",
            "Customer",
            "DeliveryMan",
            "DeliveryManLocation",
            "DeliveryVehicle",
            "MainCategory",
            "Neighborhood",
            "Notification",
            "Order",
            "OrderPackage",
            "Region",
            "Renter",
            "Resident",
            "Suggestion",
            "SystemConfiguration",
            "VehicleBrand",
            "VehicleType",
            "WalletTransctions",
        };

        private class GetAuditFilterOptionsQueryHandler : IRequestHandler<GetAuditFilterOptionsQuery, Result<AuditFilterOptionsDto>>
        {
            public Task<Result<AuditFilterOptionsDto>> Handle(GetAuditFilterOptionsQuery request, CancellationToken cancellationToken)
            {
                var entityTypes = AllAuditableEntityTypes
                    .OrderBy(x => x)
                    .ToList();

                return Task.FromResult(Result.Success(new AuditFilterOptionsDto
                {
                    ActionNames = new List<string>(),
                    EntityTypes = entityTypes
                }));
            }
        }
    }
}
