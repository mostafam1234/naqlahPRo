using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.AssistantWork.Commands
{
    public sealed record AddAssistantWorkCommand : IRequest<Result<int>>
    {
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public decimal Cost { get; set; }

        private class AddAssistantWorkCommandHandler : IRequestHandler<AddAssistantWorkCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public AddAssistantWorkCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(AddAssistantWorkCommand command, CancellationToken cancellationToken)
            {
                var assistantWork = AssistanWork.Instance(command.ArabicName, command.EnglishName, command.Cost);
                var assistantWorkValue = assistantWork.Value;
                await _context.AssistanWorks.AddAsync(assistantWorkValue);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(assistantWorkValue.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}

