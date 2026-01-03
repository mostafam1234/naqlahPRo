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

namespace Application.Features.AdminSection.CityFeatures.Commands
{
    public sealed record AddCityCommand: IRequest<Result<int>>
    {
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public int RegionId { get; set; }

        private class AddCityCommandHandler : IRequestHandler<AddCityCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public AddCityCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(AddCityCommand command, CancellationToken cancellationToken)
            {
                var regionExists = await _context.Regions.AnyAsync(x => x.Id == command.RegionId, cancellationToken);
                if (!regionExists)
                {
                    return Result.Failure<int>("Region Not Found");
                }

                var city = City.Instance(command.ArabicName, command.EnglishName, command.RegionId);
                var cityValue = city.Value;
                await _context.Cities.AddAsync(cityValue);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(cityValue.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}


