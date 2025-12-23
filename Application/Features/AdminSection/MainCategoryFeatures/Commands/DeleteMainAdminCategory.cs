using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Application.Features.AdminSection.MainCategoryFeatures.Commands
{
    public sealed record DeleteMainAdminCategory: IRequest<Result<int>>
    {
        public int Id { get; set; }
        private class DeleteMainAdminCategoryHandler : IRequestHandler<DeleteMainAdminCategory, Result<int>>
        {
            private readonly INaqlahContext _context;
            public DeleteMainAdminCategoryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(DeleteMainAdminCategory command, CancellationToken cancellationToken)
            {
                var category = await _context.MainCategories.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (category == null)
                {
                    return Result.Failure<int>("Category Not Found");
                }
                await _context.MainCategories.Where(x => x.Id == command.Id).ExecuteDeleteAsync();
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(category.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}
