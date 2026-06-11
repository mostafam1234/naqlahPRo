using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdminSection.OrderFeature.Queries
{
    public class GetOrderRatingsQuery : IRequest<Result<PagedResult<OrderRatingAdminDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int? MinRating { get; init; }
        public int? MaxRating { get; init; }

        private class Handler : IRequestHandler<GetOrderRatingsQuery, Result<PagedResult<OrderRatingAdminDto>>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context) => _context = context;

            public async Task<Result<PagedResult<OrderRatingAdminDto>>> Handle(
                GetOrderRatingsQuery request,
                CancellationToken cancellationToken)
            {
                var query = from rating in _context.OrderRatings
                            join order in _context.Orders on rating.OrderId equals order.Id
                            join customer in _context.Customers on rating.CustomerId equals customer.Id
                            join user in _context.Users on customer.UserId equals user.Id
                            select new
                            {
                                rating.Id,
                                rating.OrderId,
                                order.OrderNumber,
                                CustomerName = user.UserName ?? user.PhoneNumber ?? "—",
                                rating.DeliveryManId,
                                rating.Rating,
                                rating.Comment,
                                rating.CreatedDate
                            };

                if (request.FromDate.HasValue)
                {
                    var from = request.FromDate.Value.Date;
                    query = query.Where(x => x.CreatedDate >= from);
                }

                if (request.ToDate.HasValue)
                {
                    var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(x => x.CreatedDate <= to);
                }

                if (request.MinRating.HasValue)
                    query = query.Where(x => x.Rating >= request.MinRating.Value);

                if (request.MaxRating.HasValue)
                    query = query.Where(x => x.Rating <= request.MaxRating.Value);

                var totalCount = await query.CountAsync(cancellationToken);

                var rows = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToListAsync(cancellationToken);

                // Resolve delivery man names in a second query to avoid complex joins
                var deliveryManIds = rows
                    .Where(r => r.DeliveryManId.HasValue)
                    .Select(r => r.DeliveryManId!.Value)
                    .Distinct()
                    .ToList();

                var deliveryMenNames = deliveryManIds.Any()
                    ? await _context.DeliveryMen
                        .Where(d => deliveryManIds.Contains(d.Id))
                        .Select(d => new { d.Id, d.FullName })
                        .ToDictionaryAsync(d => d.Id, d => d.FullName, cancellationToken)
                    : new Dictionary<int, string>();

                var items = rows.Select(r => new OrderRatingAdminDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    OrderNumber = r.OrderNumber,
                    CustomerName = r.CustomerName,
                    DeliveryManName = r.DeliveryManId.HasValue && deliveryMenNames.TryGetValue(r.DeliveryManId.Value, out var name)
                        ? name
                        : null,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedDate = r.CreatedDate
                }).ToList();

                return Result.Success(new PagedResult<OrderRatingAdminDto>
                {
                    Data = items,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.Take)
                });
            }
        }
    }
}
