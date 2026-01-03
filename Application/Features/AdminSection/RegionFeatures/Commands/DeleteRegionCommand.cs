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
    public sealed record DeleteRegionCommand: IRequest<Result<int>>
    {
        public int Id { get; set; }
        private class DeleteRegionCommandHandler : IRequestHandler<DeleteRegionCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public DeleteRegionCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(DeleteRegionCommand command, CancellationToken cancellationToken)
            {
                var region = await _context.Regions.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (region == null)
                {
                    return Result.Failure<int>("Region Not Found");
                }
                await _context.Regions.Where(x => x.Id == command.Id).ExecuteDeleteAsync();
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


