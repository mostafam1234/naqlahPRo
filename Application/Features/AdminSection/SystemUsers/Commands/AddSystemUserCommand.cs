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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.SystemUsers.Commands
{
    public sealed record AddSystemUserCommand : IRequest<Result<int>>
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;

        private class AddSystemUserCommandHandler : IRequestHandler<AddSystemUserCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly UserManager<Domain.Models.User> _userManager;
            private readonly IUserService _userService;

            public AddSystemUserCommandHandler(
                INaqlahContext context,
                UserManager<Domain.Models.User> userManager,
                IUserService userService)
            {
                _context = context;
                _userManager = userManager;
                _userService = userService;
            }

            public async Task<Result<int>> Handle(AddSystemUserCommand command, CancellationToken cancellationToken)
            {
                // Check if email already exists
                var isEmailExists = await _context.Users
                    .AnyAsync(x => x.Email == command.Email, cancellationToken);

                if (isEmailExists)
                {
                    return Result.Failure<int>("البريد الإلكتروني مستخدم بالفعل");
                }

                // Validate phone number format (KSA: starts with 05, exactly 10 digits)
                if (string.IsNullOrWhiteSpace(command.PhoneNumber))
                {
                    return Result.Failure<int>("رقم الهاتف مطلوب");
                }

                // Remove any spaces or special characters for validation
                var cleanPhoneNumber = command.PhoneNumber.Trim().Replace(" ", "").Replace("-", "");
                
                // Validate KSA phone number format: must start with 05 and be exactly 10 digits
                if (!Regex.IsMatch(cleanPhoneNumber, @"^05\d{8}$"))
                {
                    return Result.Failure<int>("رقم الهاتف غير صحيح. يجب أن يبدأ بـ 05 ويتكون من 10 أرقام (مثال: 0512345678)");
                }

                // Check if phone number already exists
                var isPhoneExists = await _context.Users
                    .AnyAsync(x => x.PhoneNumber == cleanPhoneNumber, cancellationToken);

                if (isPhoneExists)
                {
                    return Result.Failure<int>("رقم الهاتف مستخدم بالفعل");
                }

                // Use cleaned phone number
                command.PhoneNumber = cleanPhoneNumber;

                // Check if username already exists
                var isUserNameExists = await _context.Users
                    .AnyAsync(x => x.UserName == command.UserName, cancellationToken);

                if (isUserNameExists)
                {
                    return Result.Failure<int>("اسم المستخدم مستخدم بالفعل");
                }

                // Create new user
                var user = new Domain.Models.User
                {
                    UserName = command.UserName,
                    Email = command.Email,
                    PhoneNumber = command.PhoneNumber,
                    IsActive = command.IsActive,
                    IsDeleted = false
                };

                // Create user with password
                var createResult = await _userManager.CreateAsync(user, command.Password);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.ToList();
                    var errorMessage = string.Join(", ", errors.Select(e => e.Description));
                    return Result.Failure<int>(errorMessage);
                }

                // Validate role exists
                var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == command.RoleId, cancellationToken);
                if (role == null)
                {
                    await _userManager.DeleteAsync(user);
                    return Result.Failure<int>("الدور المحدد غير موجود");
                }

                // Assign role using Identity first (this will create the UserRole record in NA_UserRoles)
                var roleResult = await _userManager.AddToRoleAsync(user, role.Name!);
                if (!roleResult.Succeeded)
                {
                    // If role assignment fails, clean up and return failure
                    await _userManager.DeleteAsync(user);
                    var errors = roleResult.Errors.ToList();
                    var errorMessage = string.Join(", ", errors.Select(e => e.Description));
                    return Result.Failure<int>($"فشل في تعيين الدور: {errorMessage}");
                }

                // Verify the role assignment by checking directly in the database
                var userRoleExists = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == command.RoleId, cancellationToken);
                
                if (!userRoleExists)
                {
                    // If for some reason the UserRole wasn't created, create it explicitly
                    var userRole = Domain.Models.UserRole.Instance(command.RoleId);
                    userRole.UserId = user.Id;
                    await _context.UserRoles.AddAsync(userRole, cancellationToken);
                    var saveResult = await _context.SaveChangesAsyncWithResult();
                    if (saveResult.IsFailure)
                    {
                        await _userManager.DeleteAsync(user);
                        return Result.Failure<int>($"فشل في حفظ سجل الدور: {saveResult.Error}");
                    }
                }

                // Reload the user from the database to refresh Identity's cache
                // This ensures Identity recognizes the newly assigned role
                var reloadedUser = await _userManager.FindByIdAsync(user.Id.ToString());
                if (reloadedUser != null)
                {
                    // Verify that Identity can now see the role
                    var verifyRoles = await _userManager.GetRolesAsync(reloadedUser);
                    if (!verifyRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase) && 
                        !verifyRoles.Contains(role.NormalizedName ?? role.Name!, StringComparer.OrdinalIgnoreCase))
                    {
                        // If Identity still doesn't see the role, the database check in login will handle it
                        // This is acceptable as we have a fallback check in GetAdminAccessToken
                    }
                }

                // Store FullName as claim
                if (!string.IsNullOrWhiteSpace(command.FullName))
                {
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", command.FullName));
                }

                return Result.Success(user.Id);
            }
        }
    }
}

