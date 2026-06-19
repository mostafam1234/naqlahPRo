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
    /// <summary>Restores a soft-deleted customer identity user (IsDeleted = false).</summary>
    public sealed record RestoreCustomerUserCommand : IRequest<Result<int>>
    {
        public int CustomerId { get; init; }

        private class RestoreCustomerUserCommandHandler : IRequestHandler<RestoreCustomerUserCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly UserManager<User> _userManager;

            public RestoreCustomerUserCommandHandler(INaqlahContext context, UserManager<User> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Result<int>> Handle(RestoreCustomerUserCommand request, CancellationToken cancellationToken)
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

                if (customer == null)
                {
                    return Result.Failure<int>("CustomerNotFound");
                }

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == customer.UserId, cancellationToken);

                if (user == null)
                {
                    return Result.Failure<int>("UserNotFound");
                }

                if (!user.IsDeleted)
                {
                    return Result.Failure<int>("AccountNotDeleted");
                }

                var roleOk = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == Role.Customer.Id, cancellationToken);

                if (!roleOk)
                {
                    return Result.Failure<int>("RecordNotCustomerAccount");
                }

                user.IsDeleted = false;

                var upd = await _userManager.UpdateAsync(user);
                if (!upd.Succeeded)
                {
                    var msg = string.Join(", ", upd.Errors.Select(e => e.Description));
                    return Result.Failure<int>(msg);
                }

                return Result.Success(user.Id);
            }
        }
    }
}
