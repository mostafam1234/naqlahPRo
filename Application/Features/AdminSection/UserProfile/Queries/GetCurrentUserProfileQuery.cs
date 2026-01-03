using Application.Features.AdminSection.UserProfile.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.UserProfile.Queries
{
    public sealed record GetCurrentUserProfileQuery : IRequest<Result<UserProfileDto>>
    {
        private class GetCurrentUserProfileQueryHandler : IRequestHandler<GetCurrentUserProfileQuery, Result<UserProfileDto>>
        {
            private readonly INaqlahContext _context;
            private readonly IUserSession _userSession;

            public GetCurrentUserProfileQueryHandler(INaqlahContext context, IUserSession userSession)
            {
                _context = context;
                _userSession = userSession;
            }

            public async Task<Result<UserProfileDto>> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
            {
                if (_userSession.UserId == 0)
                {
                    return Result.Failure<UserProfileDto>("User not authenticated");
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == _userSession.UserId && !u.IsDeleted, cancellationToken);

                if (user == null)
                {
                    return Result.Failure<UserProfileDto>("User not found");
                }

                // Get user role from UserRoles table
                var userRole = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r)
                    .FirstOrDefaultAsync(cancellationToken);

                var profile = new UserProfileDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    IsActive = user.IsActive,
                    RoleId = userRole?.Id ?? 0,
                    RoleName = userRole?.Name ?? string.Empty,
                    RoleArabicName = userRole?.ArabicName ?? string.Empty,
                    FullName = user.UserName ?? string.Empty // You can add FullName to User model if needed
                };

                return Result.Success(profile);
            }
        }
    }
}

