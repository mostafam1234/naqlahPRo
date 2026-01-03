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
    public sealed record UpdateNeighborhoodCommand: IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public int CityId { get; set; }
        private class UpdateNeighborhoodCommandHandler : IRequestHandler<UpdateNeighborhoodCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public UpdateNeighborhoodCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(UpdateNeighborhoodCommand command, CancellationToken cancellationToken)
            {
                var neighborhood = await _context.Neighborhoods.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (neighborhood == null)
                {
                    return Result.Failure<int>("Neighborhood Not Found");
                }

                var cityExists = await _context.Cities.AnyAsync(x => x.Id == command.CityId, cancellationToken);
                if (!cityExists)
                {
                    return Result.Failure<int>("City Not Found");
                }

                neighborhood.Update(command.ArabicName, command.EnglishName, command.CityId);
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


