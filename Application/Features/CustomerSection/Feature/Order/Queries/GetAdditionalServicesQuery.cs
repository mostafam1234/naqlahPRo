using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CustomerSection.Feature.Order.Queries
{
    public sealed record GetAdditionalServicesQuery : IRequest<Result<List<AdditionalServiceDto>>>
    {
        private class GetAdditionalServicesQueryHandler : IRequestHandler<GetAdditionalServicesQuery,
                                                                          Result<List<AdditionalServiceDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetAdditionalServicesQueryHandler(INaqlahContext context, IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<List<AdditionalServiceDto>>> Handle(GetAdditionalServicesQuery request,
                                                                         CancellationToken cancellationToken)
            {
                var languageId = userSession.LanguageId;

                var services = await context.AdditionalServices
                    .Where(x => !x.IsDeleted)
                    .Select(x => new AdditionalServiceDto
                    {
                        Id = x.Id,
                        Name = languageId == (int)Language.Arabic ? x.ArabicName : x.EnglishName,
                        UnitPrice = x.UnitPrice
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync(cancellationToken);

                return Result.Success(services);
            }
        }
    }

    public class AdditionalServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }
}
