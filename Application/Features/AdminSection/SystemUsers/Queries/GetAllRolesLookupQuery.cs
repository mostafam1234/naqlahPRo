using Application.Features.AdminSection.SystemUsers.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.SystemUsers.Queries
{
    public sealed record GetAllRolesLookupQuery : IRequest<Result<List<RoleLookupDto>>>
    {
        private class GetAllRolesLookupQueryHandler : IRequestHandler<GetAllRolesLookupQuery, Result<List<RoleLookupDto>>>
        {
            private readonly INaqlahContext _context;

            public GetAllRolesLookupQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<List<RoleLookupDto>>> Handle(GetAllRolesLookupQuery request, CancellationToken cancellationToken)
            {
                // Get Customer and DeliveryMan role IDs to exclude
                var customerRoleId = Domain.Models.Role.Customer.Id; // 3
                var deliveryManRoleId = Domain.Models.Role.DeliveryMan.Id; // 2

                var roles = await _context.Roles
                    .Where(x => x.Id != customerRoleId && x.Id != deliveryManRoleId)
                    .Select(x => new RoleLookupDto
                    {
                        Id = x.Id,
                        Name = x.Name ?? string.Empty,
                        ArabicName = x.ArabicName ?? string.Empty
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(roles);
            }
        }
    }
}

