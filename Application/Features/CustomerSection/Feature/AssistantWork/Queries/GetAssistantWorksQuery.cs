using Application.Features.CustomerSection.Feature.AssistantWork.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.AssistantWork.Queries
{
    public sealed record GetAssistantWorksQuery : IRequest<Result<List<AssistantWorkDto>>>
    {
        private class GetAssistantWorksQueryHandler : IRequestHandler<GetAssistantWorksQuery,
                                                                      Result<List<AssistantWorkDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetAssistantWorksQueryHandler(INaqlahContext context,
                                                 IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<List<AssistantWorkDto>>> Handle(GetAssistantWorksQuery request, CancellationToken cancellationToken)
            {
                var languageId = userSession.LanguageId;

                var assistantWorks = await context.AssistanWorks
                    .Where(aw => !aw.IsDeleted)
                    .Select(aw => new AssistantWorkDto
                    {
                        Id = aw.Id,
                        Name = languageId == (int)Language.Arabic ?
                               aw.ArabicName : aw.EnglishName
                    })
                    .OrderBy(aw => aw.Name)
                    .ToListAsync(cancellationToken);

                return Result.Success(assistantWorks);
            }
        }
    }
}