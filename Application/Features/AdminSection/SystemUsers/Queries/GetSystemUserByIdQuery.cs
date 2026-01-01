using Application.Features.AdminSection.SystemUsers.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.SystemUsers.Queries
{
    public sealed record GetSystemUserByIdQuery : IRequest<Result<SystemUserAdminDto>>
    {
        public int Id { get; init; }

        private class GetSystemUserByIdQueryHandler : IRequestHandler<GetSystemUserByIdQuery, Result<SystemUserAdminDto>>
        {
            private readonly INaqlahContext _context;
            private readonly UserManager<Domain.Models.User> _userManager;

            public GetSystemUserByIdQueryHandler(
                INaqlahContext context,
                UserManager<Domain.Models.User> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Result<SystemUserAdminDto>> Handle(GetSystemUserByIdQuery request, CancellationToken cancellationToken)
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

                if (user == null)
                {
                    return Result.Failure<SystemUserAdminDto>("المستخدم غير موجود");
                }

                // Get user role from UserRoles table
                var userRole = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r)
                    .FirstOrDefaultAsync(cancellationToken);

                if (userRole == null)
                {
                    return Result.Failure<SystemUserAdminDto>("المستخدم ليس لديه دور محدد");
                }

                var claims = await _userManager.GetClaimsAsync(user);
                var fullNameClaim = claims.FirstOrDefault(c => c.Type == "FullName");

                var dto = new SystemUserAdminDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    IsActive = user.IsActive,
                    RoleId = userRole.Id,
                    RoleName = userRole.Name ?? string.Empty,
                    RoleArabicName = userRole.ArabicName ?? string.Empty,
                    FullName = fullNameClaim?.Value ?? string.Empty
                };

                return Result.Success(dto);
            }
        }
    }
}

