using Application.Features.CustomerSection.Feature.Order.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CustomerSection.Feature.Order.Commands
{
    public sealed record RateOrderCommand : IRequest<Result<RateOrderResponseDto>>
    {
        public int OrderId { get; init; }
        public int Rating { get; init; }
        public string? Comment { get; init; }

        private class Handler : IRequestHandler<RateOrderCommand, Result<RateOrderResponseDto>>
        {
            private readonly INaqlahContext _context;
            private readonly IUserSession _userSession;
            private readonly IDateTimeProvider _dateTimeProvider;

            public Handler(INaqlahContext context, IUserSession userSession, IDateTimeProvider dateTimeProvider)
            {
                _context = context;
                _userSession = userSession;
                _dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result<RateOrderResponseDto>> Handle(
                RateOrderCommand request,
                CancellationToken cancellationToken)
            {
                if (request.Rating < 1 || request.Rating > 5)
                    return Result.Failure<RateOrderResponseDto>("Rating must be between 1 and 5.");

                var customerId = await _context.Customers
                    .Where(c => c.UserId == _userSession.UserId)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                    return Result.Failure<RateOrderResponseDto>("Customer not found.");

                var order = await _context.Orders
                    .Where(o => o.Id == request.OrderId && o.CustomerId == customerId)
                    .Select(o => new { o.Id, o.OrderStatus, o.DeliveryManId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (order is null)
                    return Result.Failure<RateOrderResponseDto>("Order not found or access denied.");

                if (order.OrderStatus != OrderStatus.Completed)
                    return Result.Failure<RateOrderResponseDto>("Only completed orders can be rated.");

                var alreadyRated = await _context.OrderRatings
                    .AnyAsync(r => r.OrderId == request.OrderId && r.CustomerId == customerId, cancellationToken);

                if (alreadyRated)
                    return Result.Failure<RateOrderResponseDto>("You have already submitted a rating for this order.");

                var ratingResult = Domain.Models.OrderRating.Instance(
                    orderId: request.OrderId,
                    customerId: customerId,
                    deliveryManId: order.DeliveryManId,
                    rating: request.Rating,
                    comment: request.Comment,
                    createdDate: _dateTimeProvider.Now);

                if (ratingResult.IsFailure)
                    return Result.Failure<RateOrderResponseDto>(ratingResult.Error);

                await _context.OrderRatings.AddAsync(ratingResult.Value, cancellationToken);

                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure<RateOrderResponseDto>(saveResult.Error);

                return Result.Success(new RateOrderResponseDto
                {
                    Success = true,
                    Message = "Rating submitted successfully."
                });
            }
        }
    }
}
