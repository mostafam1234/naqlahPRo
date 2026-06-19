using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.CustomerAdminFeature.Commands
{
    public sealed record AdminResetCustomerPasswordCommand : IRequest<Result<int>>
    {
        public int CustomerId { get; init; }
        public string NewPassword { get; init; } = string.Empty;

        private class AdminResetCustomerPasswordCommandHandler : IRequestHandler<AdminResetCustomerPasswordCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly UserManager<User> _userManager;

            public AdminResetCustomerPasswordCommandHandler(INaqlahContext context, UserManager<User> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Result<int>> Handle(AdminResetCustomerPasswordCommand request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return Result.Failure<int>("NewPasswordRequired");
                }

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

                if (customer == null)
                {
                    return Result.Failure<int>("CustomerNotFound");
                }

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == customer.UserId, cancellationToken);

                if (user == null || user.IsDeleted)
                {
                    return Result.Failure<int>("UserNotFoundOrDeleted");
                }

                var roleOk = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == Role.Customer.Id, cancellationToken);

                if (!roleOk)
                {
                    return Result.Failure<int>("RecordNotCustomerAccount");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    var errors = passwordResult.Errors.Select(e => e.Description);
                    return Result.Failure<int>(string.Join(", ", errors));
                }

                return Result.Success(user.Id);
            }
        }
    }
}
