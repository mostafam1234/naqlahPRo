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

namespace Application.Features.DeliveryManSection.Order.Commands
{
    public sealed record ChangeOrderWayPointStatusCommand : IRequest<Result>
    {
        public int OrderId { get; set; }
        public int WayPointId { get; set; }
        public string PackImageBase64 { get; set; } = string.Empty;
    }

    internal class ChangeOrderWayPointStatusCommandHandler : IRequestHandler<ChangeOrderWayPointStatusCommand, Result>
    {
        private readonly INaqlahContext context;
        private readonly IUserSession userSession;
        private readonly IMediaUploader mediaUploader;
        private readonly IDateTimeProvider dateTimeProvider;
        private const string DeliveryOrderFolderPrefix = "DeliveryOrders";

        public ChangeOrderWayPointStatusCommandHandler(
            INaqlahContext context,
            IUserSession userSession,
            IMediaUploader mediaUploader,
            IDateTimeProvider dateTimeProvider)
        {
            this.context = context;
            this.userSession = userSession;
            this.mediaUploader = mediaUploader;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(ChangeOrderWayPointStatusCommand request, CancellationToken cancellationToken)
        {
            var userId = userSession.UserId;
            
            // Get the delivery man
            var deliveryMan = await context.DeliveryMen
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (deliveryMan is null)
            {
                return Result.Failure("Delivery man not found");
            }

            // Get the order with waypoints and status history
            var order = await context.Orders
                .Include(o => o.OrderWayPoints)
                .Include(o => o.OrderStatusHistories)
                .AsTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.DeliveryManId == deliveryMan.Id, cancellationToken);

            if (order is null)
            {
                return Result.Failure("Order not found or not assigned to you");
            }

            // Find the specific waypoint
            var wayPoint = order.OrderWayPoints.FirstOrDefault(wp => wp.Id == request.WayPointId);
            
            if (wayPoint is null)
            {
                return Result.Failure("Waypoint not found in this order");
            }

            // Check if the order is in the right status (should be Assigned)
            if (order.OrderStatus != OrderStatus.Assigned)
            {
                return Result.Failure($"Cannot update waypoint. Order is in {order.OrderStatus} status");
            }

            // Upload the pack image
            var orderFolder = $"{DeliveryOrderFolderPrefix}/Order_{order.Id}";
            var packImagePath = await mediaUploader.UploadFromBase64(request.PackImageBase64, orderFolder);

            if (string.IsNullOrWhiteSpace(packImagePath))
            {
                return Result.Failure("Failed to upload pack image");
            }

            // Mark the waypoint as picked up
            var currentDateTime = dateTimeProvider.Now;
            var markResult = wayPoint.MarkAsPickedUp(packImagePath, currentDateTime);

            if (markResult.IsFailure)
            {
                return Result.Failure(markResult.Error);
            }

            // Check if all waypoints are picked up and auto-complete the order if needed
            var checkCompleteResult = order.CheckAndCompleteIfAllWayPointsPickedUp(currentDateTime);
            
            if (checkCompleteResult.IsFailure)
            {
                return Result.Failure(checkCompleteResult.Error);
            }

            // Save changes to database
            var saveResult = await context.SaveChangesAsyncWithResult();
            
            if (saveResult.IsFailure)
            {
                return Result.Failure(saveResult.Error);
            }

            return Result.Success();
        }
    }
}