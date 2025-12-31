using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.SystemUsers.Commands
{
    public sealed record UpdateSystemUserCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public string? NewPassword { get; set; }

        private class UpdateSystemUserCommandHandler : IRequestHandler<UpdateSystemUserCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly UserManager<Domain.Models.User> _userManager;

            public UpdateSystemUserCommandHandler(
                INaqlahContext context,
                UserManager<Domain.Models.User> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Result<int>> Handle(UpdateSystemUserCommand command, CancellationToken cancellationToken)
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

                if (user == null)
                {
                    return Result.Failure<int>("المستخدم غير موجود");
                }

                // Check if email is being changed and if new email already exists
                if (user.Email != command.Email)
                {
                    var isEmailExists = await _context.Users
                        .AnyAsync(x => x.Email == command.Email && x.Id != command.Id, cancellationToken);

                    if (isEmailExists)
                    {
                        return Result.Failure<int>("البريد الإلكتروني مستخدم بالفعل");
                    }
                    user.Email = command.Email;
                }

                // Check if phone number is being changed and if new phone already exists
                if (user.PhoneNumber != command.PhoneNumber && !string.IsNullOrWhiteSpace(command.PhoneNumber))
                {
                    var isPhoneExists = await _context.Users
                        .AnyAsync(x => x.PhoneNumber == command.PhoneNumber && x.Id != command.Id, cancellationToken);

                    if (isPhoneExists)
                    {
                        return Result.Failure<int>("رقم الهاتف مستخدم بالفعل");
                    }
                    user.PhoneNumber = command.PhoneNumber;
                }

                user.IsActive = command.IsActive;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(command.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, command.NewPassword);
                    if (!passwordResult.Succeeded)
                    {
                        var errors = passwordResult.Errors.ToList();
                        var errorMessage = string.Join(", ", errors.Select(e => e.Description));
                        return Result.Failure<int>($"فشل في تحديث كلمة المرور: {errorMessage}");
                    }
                }

                // Update role if changed
                var currentRoles = await _userManager.GetRolesAsync(user);
                var newRole = await _context.Roles.FirstOrDefaultAsync(x => x.Id == command.RoleId, cancellationToken);
                if (newRole == null)
                {
                    return Result.Failure<int>("الدور المحدد غير موجود");
                }

                // Remove user from all current roles
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                // Add user to new role
                var roleResult = await _userManager.AddToRoleAsync(user, newRole.Name!);
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.ToList();
                    var errorMessage = string.Join(", ", errors.Select(e => e.Description));
                    return Result.Failure<int>($"فشل في تحديث الدور: {errorMessage}");
                }

                // Update claims
                var existingClaims = await _userManager.GetClaimsAsync(user);
                var fullNameClaim = existingClaims.FirstOrDefault(c => c.Type == "FullName");

                if (fullNameClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, fullNameClaim);
                }
                if (!string.IsNullOrWhiteSpace(command.FullName))
                {
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", command.FullName));
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors.ToList();
                    var errorMessage = string.Join(", ", errors.Select(e => e.Description));
                    return Result.Failure<int>(errorMessage);
                }

                return Result.Success(user.Id);
            }
        }
    }
}

