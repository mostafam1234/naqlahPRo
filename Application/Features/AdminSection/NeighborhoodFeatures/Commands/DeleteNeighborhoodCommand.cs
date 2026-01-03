using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.NeighborhoodFeatures.Commands
{
    public sealed record DeleteNeighborhoodCommand: IRequest<Result<int>>
    {
        public int Id { get; set; }
        private class DeleteNeighborhoodCommandHandler : IRequestHandler<DeleteNeighborhoodCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public DeleteNeighborhoodCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(DeleteNeighborhoodCommand command, CancellationToken cancellationToken)
            {
                var neighborhood = await _context.Neighborhoods.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (neighborhood == null)
                {
                    return Result.Failure<int>("Neighborhood Not Found");
                }
                await _context.Neighborhoods.Where(x => x.Id == command.Id).ExecuteDeleteAsync();
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(neighborhood.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}


