using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.MainCategoryFeatures.Commands
{
    public sealed record AddMainAdminCategory: IRequest<Result<int>>
    {
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;

        private class AddMainAdminCategoryHandler : IRequestHandler<AddMainAdminCategory, Result<int>>
        {
            private readonly INaqlahContext _context;
            public AddMainAdminCategoryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(AddMainAdminCategory command, CancellationToken cancellationToken)
            {
                var category = MainCategory.Instance(command.ArabicName, command.EnglishName);
                var categoryValue = category.Value;
                await _context.MainCategories.AddAsync(categoryValue);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(categoryValue.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}
