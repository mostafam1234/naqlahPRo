using Application.Features.AdminSection.DeliveryManFeature.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record GetDeliveryManActiveHistoryQuery : IRequest<Result<DeliveryManActiveHistoryResponseDto>>
    {
        public int DeliveryManId { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<GetDeliveryManActiveHistoryQuery, Result<DeliveryManActiveHistoryResponseDto>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<DeliveryManActiveHistoryResponseDto>> Handle(
                GetDeliveryManActiveHistoryQuery request,
                CancellationToken cancellationToken)
            {
                var captain = await _context.DeliveryMen
                    .AsNoTracking()
                    .Where(dm => dm.Id == request.DeliveryManId)
                    .Select(dm => new
                    {
                        dm.Id,
                        dm.FullName,
                        dm.Active
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (captain is null)
                    return Result.Failure<DeliveryManActiveHistoryResponseDto>("DeliveryManNotFound");

                var rawHistory = await (
                    from item in _context.DeliveryManActiveHistories.AsNoTracking()
                    join user in _context.Users.AsNoTracking() on item.ChangedByUserId equals user.Id into userGroup
                    from user in userGroup.DefaultIfEmpty()
                    where item.DeliveryManId == request.DeliveryManId
                    orderby item.ChangedAt descending
                    select new
                    {
                        item.Id,
                        item.Active,
                        item.ChangedAt,
                        item.ChangedByUserId,
                        ChangedByUserName = user != null ? user.UserName : null
                    }).ToListAsync(cancellationToken);

                var history = rawHistory.Select(item => new DeliveryManActiveHistoryDto
                {
                    Id = item.Id,
                    Active = item.Active,
                    ActiveStatusName = DeliveryManDisplayLabels.GetActiveStatusName(item.Active, request.LanguageId),
                    ChangedAt = item.ChangedAt,
                    ChangedByUserId = item.ChangedByUserId,
                    ChangedByUserName = item.ChangedByUserName
                }).ToList();

                return Result.Success(new DeliveryManActiveHistoryResponseDto
                {
                    DeliveryManId = captain.Id,
                    FullName = captain.FullName,
                    CurrentActive = captain.Active,
                    CurrentActiveStatusName = DeliveryManDisplayLabels.GetActiveStatusName(captain.Active, request.LanguageId),
                    History = history
                });
            }
        }
    }
}
