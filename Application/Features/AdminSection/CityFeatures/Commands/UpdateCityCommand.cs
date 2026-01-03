using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.CityFeatures.Commands
{
    public sealed record UpdateCityCommand: IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public int RegionId { get; set; }
        private class UpdateCityCommandHandler : IRequestHandler<UpdateCityCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public UpdateCityCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(UpdateCityCommand command, CancellationToken cancellationToken)
            {
                var city = await _context.Cities.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (city == null)
                {
                    return Result.Failure<int>("City Not Found");
                }

                var regionExists = await _context.Regions.AnyAsync(x => x.Id == command.RegionId, cancellationToken);
                if (!regionExists)
                {
                    return Result.Failure<int>("Region Not Found");
                }

                city.Update(command.ArabicName, command.EnglishName, command.RegionId);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(city.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}


