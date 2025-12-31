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

                // Check if phone number already exists
                if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
                {
                    var isPhoneExists = await _context.Users
                        .AnyAsync(x => x.PhoneNumber == command.PhoneNumber, cancellationToken);

                    if (isPhoneExists)
                    {
                        return Result.Failure<int>("رقم الهاتف مستخدم بالفعل");
                    }
                }

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

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, role.Name!);
                if (!roleResult.Succeeded)
                {
                    // If role assignment fails, delete the user
                    await _userManager.DeleteAsync(user);
                    var errors = roleResult.Errors.ToList();
                    var errorMessage = string.Join(", ", errors.Select(e => e.Description));
                    return Result.Failure<int>($"فشل في تعيين الدور: {errorMessage}");
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

