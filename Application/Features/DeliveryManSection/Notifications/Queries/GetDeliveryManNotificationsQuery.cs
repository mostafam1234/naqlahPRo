using Application.Features.DeliveryManSection.Notifications.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Notifications.Queries
{
    public sealed record GetDeliveryManNotificationsQuery : IRequest<Result<DeliveryManNotificationsPageDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 50;

        private class Handler : IRequestHandler<GetDeliveryManNotificationsQuery, Result<DeliveryManNotificationsPageDto>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public Handler(INaqlahContext context, IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<DeliveryManNotificationsPageDto>> Handle(GetDeliveryManNotificationsQuery request,
                                                                              CancellationToken cancellationToken)
            {
                var deliveryManId = await context.DeliveryMen
                    .Where(x => x.UserId == userSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (deliveryManId == 0)
                {
                    return Result.Failure<DeliveryManNotificationsPageDto>("Delivery man not found");
                }

                var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
                var pageSize = request.PageSize < 1 ? 50 : request.PageSize;

                var baseQuery = context.CaptainNotifications
                    .Where(x => x.DeliveryManId == deliveryManId);

                var totalCount = await baseQuery.CountAsync(cancellationToken);

                var items = await baseQuery
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new DeliveryManNotificationDto
                    {
                        Id = x.Id,
                        Title = x.Title,
                        TitleAr = x.Title,
                        Body = x.Body,
                        BodyAr = x.Body,
                        Type = x.Type,
                        NotificationType = x.Type,
                        IsRead = x.IsRead,
                        OrderId = x.OrderId,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(new DeliveryManNotificationsPageDto
                {
                    Items = items,
                    TotalCount = totalCount
                });
            }
        }
    }
}
