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
        public string ImageBase64 { get; set; } = string.Empty;

        private class AddMainAdminCategoryHandler : IRequestHandler<AddMainAdminCategory, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly IMediaUploader _mediaUploader;
            
            public AddMainAdminCategoryHandler(INaqlahContext context, IMediaUploader mediaUploader)
            {
                _context = context;
                _mediaUploader = mediaUploader;
            }
            
            public async Task<Result<int>> Handle(AddMainAdminCategory command, CancellationToken cancellationToken)
            {
                // Validate image
                if (string.IsNullOrWhiteSpace(command.ImageBase64))
                {
                    return Result.Failure<int>("Image is required");
                }

                // Upload image
                string imagePath;
                try
                {
                    imagePath = await _mediaUploader.UploadFromBase64(command.ImageBase64, "main-categories");
                    if (string.IsNullOrWhiteSpace(imagePath))
                    {
                        return Result.Failure<int>("Failed to upload image");
                    }
                }
                catch (Exception ex)
                {
                    return Result.Failure<int>($"Failed to upload image: {ex.Message}");
                }

                var category = MainCategory.Instance(command.ArabicName, command.EnglishName, imagePath);
                if (category.IsFailure)
                {
                    return Result.Failure<int>(category.Error);
                }
                
                var categoryValue = category.Value;
                await _context.MainCategories.AddAsync(categoryValue, cancellationToken);
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
