using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.NotificationFeature.Queries
{
    public sealed record GetUnreadNotificationsCountQuery : IRequest<Result<int>>
    {
        private class GetUnreadNotificationsCountQueryHandler : IRequestHandler<GetUnreadNotificationsCountQuery, Result<int>>
        {
            private readonly INaqlahContext _context;

            public GetUnreadNotificationsCountQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(GetUnreadNotificationsCountQuery request, CancellationToken cancellationToken)
            {
                var count = await _context.Notifications
                    .CountAsync(n => !n.IsRead, cancellationToken);

                return Result.Success(count);
            }
        }
    }
}

