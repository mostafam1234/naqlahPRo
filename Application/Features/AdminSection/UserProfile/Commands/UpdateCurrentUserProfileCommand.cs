using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.UserProfile.Commands
{
    public sealed record UpdateCurrentUserProfileCommand : IRequest<Result<int>>
    {
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string? NewPassword { get; init; }

        private class UpdateCurrentUserProfileCommandHandler : IRequestHandler<UpdateCurrentUserProfileCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly IUserSession _userSession;
            private readonly UserManager<User> _userManager;

            public UpdateCurrentUserProfileCommandHandler(INaqlahContext context, IUserSession userSession, UserManager<User> userManager)
            {
                _context = context;
                _userSession = userSession;
                _userManager = userManager;
            }

            public async Task<Result<int>> Handle(UpdateCurrentUserProfileCommand request, CancellationToken cancellationToken)
            {
                if (_userSession.UserId == 0)
                {
                    return Result.Failure<int>("User not authenticated");
                }

                var user = await _context.Users
                    .AsTracking()
                    .FirstOrDefaultAsync(u => u.Id == _userSession.UserId, cancellationToken);

                if (user == null)
                {
                    return Result.Failure<int>("User not found");
                }

                // Validation
                if (string.IsNullOrWhiteSpace(request.UserName))
                {
                    return Result.Failure<int>("Username is required");
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Result.Failure<int>("Email is required");
                }

                // Check if username is already taken by another user
                var usernameExists = await _context.Users
                    .AnyAsync(u => u.UserName == request.UserName && u.Id != _userSession.UserId, cancellationToken);

                if (usernameExists)
                {
                    return Result.Failure<int>("Username is already taken");
                }

                // Check if email is already taken by another user
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == request.Email && u.Id != _userSession.UserId, cancellationToken);

                if (emailExists)
                {
                    return Result.Failure<int>("Email is already taken");
                }

                // Update user properties
                user.UserName = request.UserName;
                user.NormalizedUserName = request.UserName.ToUpper();
                user.Email = request.Email;
                user.NormalizedEmail = request.Email.ToUpper();
                user.PhoneNumber = request.PhoneNumber;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                    if (!passwordResult.Succeeded)
                    {
                        var errors = passwordResult.Errors.ToList();
                        var errorMessage = string.Join(", ", errors.Select(e => e.Description));
                        return Result.Failure<int>($"فشل في تحديث كلمة المرور: {errorMessage}");
                    }
                }

                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(user.Id);
                }

                return Result.Failure<int>(result.Error);
            }
        }
    }
}

