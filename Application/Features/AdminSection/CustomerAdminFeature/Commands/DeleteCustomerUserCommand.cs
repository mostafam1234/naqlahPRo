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
    /// <summary>Soft-deletes the customer identity user (same pattern as system users).</summary>
    public sealed record DeleteCustomerUserCommand : IRequest<Result<int>>
    {
        public int CustomerId { get; init; }

        private class DeleteCustomerUserCommandHandler : IRequestHandler<DeleteCustomerUserCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly UserManager<User> _userManager;

            public DeleteCustomerUserCommandHandler(INaqlahContext context, UserManager<User> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Result<int>> Handle(DeleteCustomerUserCommand request, CancellationToken cancellationToken)
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

                var roleOk = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == Role.Customer.Id, cancellationToken);

                if (!roleOk)
                {
                    return Result.Failure<int>("RecordNotCustomerAccount");
                }

                user.IsDeleted = true;
                user.IsActive = false;

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
