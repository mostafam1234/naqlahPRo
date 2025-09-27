using CSharpFunctionalExtensions;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Order
    {
        private Order()
        {
            _OrderDetails = new List<OrderDetails>();
            _OrderWayPoints = new List<OrderWayPoint>();
            this.OrderNumber = string.Empty;
            this._PaymentMethods = new List<OrderPaymentMethod>();
            this._OrderStatusHistories = new List<OrderStatusHistory>();
            this._OrderServices = new List<OrderService>();
        }
        public int Id { get; private set; }
        public string OrderNumber { get; set; }
        public int CustomerId { get; private set; }
        public int? VehicleTypeId { get; private set; }
        public int? DeliveryManId { get; private set; }
        public int OrderPackageId { get; private set; }
        public OrderType OrderType { get; private set; }
        public OrderStatus OrderStatus { get; private set; }
        public int VehicleTypdId { get; private set; }
        public decimal Total { get; private set; }
        public OrderPackage OrderPackage { get; private set; }
        private List<OrderDetails> _OrderDetails { get; set; }
        public IReadOnlyList<OrderDetails> OrderDetails
        {
            get
            {
                return _OrderDetails;
            }
            private set
            {
                _OrderDetails = (List<OrderDetails>)value.ToList();
            }
        }


        private List<OrderService> _OrderServices { get; set; }
        public IReadOnlyList<OrderService> OrderServices
        {
            get
            {
                return _OrderServices;
            }
            private set
            {
                _OrderServices = (List<OrderService>)value.ToList();
            }
        }

        private List<OrderWayPoint> _OrderWayPoints { get; set; }
        public IReadOnlyList<OrderWayPoint> OrderWayPoints
        {
            get
            {
                return _OrderWayPoints;
            }
            private set
            {
                _OrderWayPoints = (List<OrderWayPoint>)value.ToList();
            }
        }

        private List<OrderPaymentMethod> _PaymentMethods { get; set; }
        public IReadOnlyList<OrderPaymentMethod> PaymentMethods
        {
            get
            {
                return _PaymentMethods;
            }
            private set
            {
                _PaymentMethods = (List<OrderPaymentMethod>)value.ToList();
            }
        }

        private List<OrderStatusHistory> _OrderStatusHistories { get; set; }
        public IReadOnlyList<OrderStatusHistory> OrderStatusHistories
        {
            get
            {
                return _OrderStatusHistories;
            }
            private set
            {
                _OrderStatusHistories = (List<OrderStatusHistory>)value.ToList();
            }
        }

        public static Result<Order> Create(int customerId,
                                           OrderType orderType,
                                           string orderNumber,
                                           int orderPackageId,
                                           int paymentMethodId,
                                           DateTime nowDate,
                                           List<OrderDetails> orderDetails,
                                           List<OrderWayPoint> orderWayPoints,
                                           List<OrderService> orderServices)
        {
            const int MinOrderWayPointsCount = 2;
            var fixedTotal = 100;

            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return Result.Failure<Order>("Order number is required");
            }
            if (orderDetails.Count == 0)
            {
                return Result.Failure<Order>("At least one order detail is required");
            }

            if (orderWayPoints.Count < MinOrderWayPointsCount)
            {
                return Result.Failure<Order>("At least two order way points are required");
            }

            var orderPaymentMethod = OrderPaymentMethod.Instance(paymentMethodId, fixedTotal);
            var orderStatusHistory = OrderStatusHistory.Create(OrderStatus.Pending, nowDate);

            var order = new Order
            {
                CustomerId = customerId,
                OrderType = orderType,
                OrderStatus = OrderStatus.Pending,
                OrderDetails = orderDetails,
                OrderWayPoints = orderWayPoints,
                OrderNumber = orderNumber,
                OrderPackageId = orderPackageId,
                Total = fixedTotal,
                OrderServices = orderServices
            };

            order._PaymentMethods.Add(orderPaymentMethod);
            order._OrderStatusHistories.Add(orderStatusHistory);
            return order;

        }

        public Result AssignToDeliveryMan(int deliveryManId, DateTime nowDate)
        {
            if (this.OrderStatus != OrderStatus.Pending)
            {
                return Result.Failure("Order is not in pending status");
            }

            if (this.DeliveryManId != null)
            {
                return Result.Failure("Order is already assigned to a delivery man");
            }

            if (deliveryManId <= 0)
            {
                return Result.Failure("Invalid delivery man ID");
            }

            // Update the order status and assign the delivery man
            this.DeliveryManId = deliveryManId;
            this.OrderStatus = OrderStatus.Assigned;

            // Add status history for the assignment
            var statusHistory = OrderStatusHistory.Create(OrderStatus.Assigned, nowDate);
            this._OrderStatusHistories.Add(statusHistory);

            return Result.Success();
        }

        public Result UpdateStatus(OrderStatus newStatus, DateTime nowDate)
        {
            if (this.OrderStatus == newStatus)
            {
                return Result.Failure($"Order is already in {newStatus} status");
            }

            // Add business rules for status transitions if needed
            if (!IsValidStatusTransition(this.OrderStatus, newStatus))
            {
                return Result.Failure($"Cannot transition from {this.OrderStatus} to {newStatus}");
            }

            this.OrderStatus = newStatus;

            // Add status history
            var statusHistory = OrderStatusHistory.Create(newStatus, nowDate);
            this._OrderStatusHistories.Add(statusHistory);

            return Result.Success();
        }

        public Result SetVehicleType(int vehicleTypeId)
        {
            if (vehicleTypeId <= 0)
            {
                return Result.Failure("Invalid vehicle type ID");
            }

            if (this.VehicleTypeId != null)
            {
                return Result.Failure("Vehicle type has already been selected for this order");
            }

            if (this.OrderStatus != OrderStatus.Pending)
            {
                return Result.Failure("Vehicle type can only be set for pending orders");
            }

            this.VehicleTypeId = vehicleTypeId;
            return Result.Success();
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Define valid status transitions
            return true;
        }

        public Result CheckAndCompleteIfAllWayPointsPickedUp(DateTime nowDate)
        {
            // Check if all waypoints are picked up
            var allWayPointsPickedUp = this._OrderWayPoints.All(wp => wp.OrderWayPointsStatus == OrderWayPointsStatus.PickedUp);

            if (!allWayPointsPickedUp)
            {
                return Result.Success(); // Not all waypoints are picked up yet, nothing to do
            }

            // If the order is not in Assigned status, don't auto-complete
            if (this.OrderStatus != OrderStatus.Assigned)
            {
                return Result.Success(); // Order is not in the right status for auto-completion
            }

            // Update order status to Completed
            this.OrderStatus = OrderStatus.Completed;

            // Add status history for completion
            var statusHistory = OrderStatusHistory.Create(OrderStatus.Completed, nowDate);
            this._OrderStatusHistories.Add(statusHistory);

            // Mark all waypoints as completed
            foreach (var wayPoint in this._OrderWayPoints)
            {
                wayPoint.MarkAsCompleted(nowDate);
            }

            return Result.Success();
        }

        public Result CancelOrder(DateTime nowDate)
        {
            if (this.OrderStatus == OrderStatus.Cancelled)
            {
                return Result.Failure("Order is already cancelled");
            }

            this.OrderStatus = OrderStatus.Cancelled;

            // Add status history for cancellation
            var statusHistory = OrderStatusHistory.Create(OrderStatus.Cancelled, nowDate);
            this._OrderStatusHistories.Add(statusHistory);

            return Result.Success();
        }
    }
}
