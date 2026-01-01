using Application.Features.AdminSection.NotificationFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.NotificationFeature.Queries
{
    public sealed record GetNotificationsQuery : IRequest<Result<PagedResult<NotificationDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 20;
        public bool? UnreadOnly { get; init; }
        public int LanguageId { get; init; } = 1;

        private class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PagedResult<NotificationDto>>>
        {
            private readonly INaqlahContext _context;

            public GetNotificationsQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;

                var query = _context.Notifications
                    .AsQueryable();

                // Filter by unread if requested
                if (request.UnreadOnly == true)
                {
                    query = query.Where(n => !n.IsRead);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var notifications = await query
                    .OrderByDescending(n => n.CreationDate)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        Title = isArabic ? n.ArabicTitle : n.EnglishTitle,
                        Message = isArabic ? n.ArabicMessage : n.EnglishMessage,
                        OrderId = n.OrderId,
                        NotificationType = n.NotificationType,
                        CreationDate = n.CreationDate,
                        IsRead = n.IsRead
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var result = new PagedResult<NotificationDto>
                {
                    Data = notifications,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(result);
            }
        }
    }
}

