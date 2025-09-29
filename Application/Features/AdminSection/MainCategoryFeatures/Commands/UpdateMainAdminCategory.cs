using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.MainCategoryFeatures.Commands
{
    public sealed record UpdateMainAdminCategory: IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        private class UpdateMainAdminCategoryHandler : IRequestHandler<UpdateMainAdminCategory, Result<int>>
        {
            private readonly INaqlahContext _context;
            public UpdateMainAdminCategoryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(UpdateMainAdminCategory command, CancellationToken cancellationToken)
            {
                var category = await _context.MainCategories.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (category == null)
                {
                    return Result.Failure<int>("Category Not Found");
                }
                category.Update(command.ArabicName, command.EnglishName);
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
