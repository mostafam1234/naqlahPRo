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

namespace Application.Features.AdminSection.SystemUsers.Commands
{
    public sealed record DeleteSystemUserCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }

        private class DeleteSystemUserCommandHandler : IRequestHandler<DeleteSystemUserCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly UserManager<Domain.Models.User> _userManager;

            public DeleteSystemUserCommandHandler(
                INaqlahContext context,
                UserManager<Domain.Models.User> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Result<int>> Handle(DeleteSystemUserCommand command, CancellationToken cancellationToken)
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

                if (user == null)
                {
                    return Result.Failure<int>("المستخدم غير موجود");
                }

                // Soft delete
                user.IsDeleted = true;
                user.IsActive = false;

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

