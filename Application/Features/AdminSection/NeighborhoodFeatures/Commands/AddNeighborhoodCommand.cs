using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.NeighborhoodFeatures.Commands
{
    public sealed record AddNeighborhoodCommand: IRequest<Result<int>>
    {
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public int CityId { get; set; }

        private class AddNeighborhoodCommandHandler : IRequestHandler<AddNeighborhoodCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public AddNeighborhoodCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(AddNeighborhoodCommand command, CancellationToken cancellationToken)
            {
                var cityExists = await _context.Cities.AnyAsync(x => x.Id == command.CityId, cancellationToken);
                if (!cityExists)
                {
                    return Result.Failure<int>("City Not Found");
                }

                var neighborhood = Neighborhood.Instance(command.ArabicName, command.EnglishName, command.CityId);
                var neighborhoodValue = neighborhood.Value;
                await _context.Neighborhoods.AddAsync(neighborhoodValue);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(neighborhoodValue.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}


