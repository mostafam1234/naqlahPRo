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
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string? ImageBase64 { get; set; }
        
        private class UpdateMainAdminCategoryHandler : IRequestHandler<UpdateMainAdminCategory, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly IMediaUploader _mediaUploader;
            
            public UpdateMainAdminCategoryHandler(INaqlahContext context, IMediaUploader mediaUploader)
            {
                _context = context;
                _mediaUploader = mediaUploader;
            }
            
            public async Task<Result<int>> Handle(UpdateMainAdminCategory command, CancellationToken cancellationToken)
            {
                var category = await _context.MainCategories.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
                if (category == null)
                {
                    return Result.Failure<int>("Category Not Found");
                }

                string imagePath = category.ImagePath;
                
                // Upload new image if provided
                if (!string.IsNullOrWhiteSpace(command.ImageBase64))
                {
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
                }

                category.Update(command.ArabicName, command.EnglishName, imagePath);
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
