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
    public sealed record DeleteCityCommand: IRequest<Result<int>>
    {
        public int Id { get; set; }
        private class DeleteCityCommandHandler : IRequestHandler<DeleteCityCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public DeleteCityCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(DeleteCityCommand command, CancellationToken cancellationToken)
            {
                var city = await _context.Cities.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (city == null)
                {
                    return Result.Failure<int>("City Not Found");
                }
                await _context.Cities.Where(x => x.Id == command.Id).ExecuteDeleteAsync();
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


