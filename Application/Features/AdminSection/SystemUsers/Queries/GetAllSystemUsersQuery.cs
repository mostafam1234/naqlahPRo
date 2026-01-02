using Application.Features.AdminSection.SystemUsers.Dtos;
using Application.Shared.Dtos;
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
    public sealed record GetAllSystemUsersQuery : IRequest<Result<PagedResult<SystemUserAdminDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public string? RoleFilter { get; init; }

        private class GetAllSystemUsersQueryHandler : IRequestHandler<GetAllSystemUsersQuery, Result<PagedResult<SystemUserAdminDto>>>
        {
            private readonly INaqlahContext _context;
            private readonly UserManager<Domain.Models.User> _userManager;

            public GetAllSystemUsersQueryHandler(
                INaqlahContext context,
                UserManager<Domain.Models.User> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Result<PagedResult<SystemUserAdminDto>>> Handle(GetAllSystemUsersQuery request, CancellationToken cancellationToken)
            {
                // Get all system users (users with roles)
                var query = _context.Users
                    .Where(u => !u.IsDeleted && _context.UserRoles.Any(ur => ur.UserId == u.Id))
                    .AsQueryable();

                // Apply role filter if specified
                if (!string.IsNullOrWhiteSpace(request.RoleFilter) && int.TryParse(request.RoleFilter, out int roleId))
                {
                    var userIdsWithRole = await _context.UserRoles
                        .Where(ur => ur.RoleId == roleId)
                        .Select(ur => ur.UserId)
                        .ToListAsync(cancellationToken);
                    query = query.Where(u => userIdsWithRole.Contains(u.Id));
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => 
                        x.UserName!.Contains(request.SearchTerm) ||
                        x.Email!.Contains(request.SearchTerm) ||
                        x.PhoneNumber!.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var users = await query
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToListAsync(cancellationToken);

                var userDtos = new List<SystemUserAdminDto>();

                foreach (var user in users)
                {
                    // Get user role from UserRoles table
                    var userRole = await _context.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_context.Roles,
                            ur => ur.RoleId,
                            r => r.Id,
                            (ur, r) => r)
                        .FirstOrDefaultAsync(cancellationToken);

                    var claims = await _userManager.GetClaimsAsync(user);
                    var fullNameClaim = claims.FirstOrDefault(c => c.Type == "FullName");

                    userDtos.Add(new SystemUserAdminDto
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        PhoneNumber = user.PhoneNumber ?? string.Empty,
                        IsActive = user.IsActive,
                        RoleId = userRole?.Id ?? 0,
                        RoleName = userRole?.Name ?? string.Empty,
                        RoleArabicName = userRole?.ArabicName ?? string.Empty,
                        FullName = fullNameClaim?.Value ?? string.Empty
                    });
                }

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<SystemUserAdminDto>
                {
                    Data = userDtos,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }
    }
}

