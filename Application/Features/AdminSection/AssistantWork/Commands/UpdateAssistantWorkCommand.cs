using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.AssistantWork.Commands
{
    public sealed record UpdateAssistantWorkCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public decimal Cost { get; set; }
        
        private class UpdateAssistantWorkCommandHandler : IRequestHandler<UpdateAssistantWorkCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public UpdateAssistantWorkCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(UpdateAssistantWorkCommand command, CancellationToken cancellationToken)
            {
                var assistantWork = await _context.AssistanWorks.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (assistantWork == null)
                {
                    return Result.Failure<int>("Assistant Work Not Found");
                }
                assistantWork.Update(command.ArabicName, command.EnglishName, command.Cost);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(assistantWork.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}

