using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.RegionFeatures.Commands
{
    public sealed record AddRegionCommand: IRequest<Result<int>>
    {
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;

        private class AddRegionCommandHandler : IRequestHandler<AddRegionCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public AddRegionCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(AddRegionCommand command, CancellationToken cancellationToken)
            {
                var region = Region.Instance(command.ArabicName, command.EnglishName);
                var regionValue = region.Value;
                await _context.Regions.AddAsync(regionValue);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(regionValue.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}


