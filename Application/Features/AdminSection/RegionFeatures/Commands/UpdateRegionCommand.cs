using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.RegionFeatures.Commands
{
    public sealed record UpdateRegionCommand: IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        private class UpdateRegionCommandHandler : IRequestHandler<UpdateRegionCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public UpdateRegionCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(UpdateRegionCommand command, CancellationToken cancellationToken)
            {
                var region = await _context.Regions.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (region == null)
                {
                    return Result.Failure<int>("Region Not Found");
                }
                region.Update(command.ArabicName, command.EnglishName);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(region.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}


