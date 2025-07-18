using Application.Features.DeliveryManSection.Assistant.Dtos;
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

namespace Application.Features.DeliveryManSection.Assistant.Queries
{
    public sealed record GetAssistantWorksQuery:IRequest<Result<List<MaidTypeDto>>>
    {
        private class GetAssistantWorksQueryHandler :
            IRequestHandler<GetAssistantWorksQuery, Result<List<MaidTypeDto>>>
        {
            private readonly IUserSession userSession;
            private readonly INaqlahContext context;

            public GetAssistantWorksQueryHandler(IUserSession userSession,
                                                 INaqlahContext context)
            {
                this.userSession = userSession;
                this.context = context;
            }
            public async Task<Result<List<MaidTypeDto>>> Handle(GetAssistantWorksQuery request, CancellationToken cancellationToken)
            {
                var works = await context.AssistanWorks
                                       .Where(x => !x.IsDeleted)
                                       .Select(x => new MaidTypeDto
                                       {
                                           Id = x.Id,
                                           Name = userSession.LanguageId == (int)Language.Arabic ?
                                           x.ArabicName : x.EnglishName
                                       }).ToListAsync();

                return works;
            }
        }
    }
}
