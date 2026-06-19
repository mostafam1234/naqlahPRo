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
            this._OrderAdditionalServices = new List<OrderAdditionalService>();
            this.HashedLocation = string.Empty;
        }
        public int Id { get; private set; }
        public string OrderNumber { get; set; }
        public int CustomerId { get; private set; }
        public DateTime CreationDate { get;private set; }
        public double ExpectedTimeMinutes { get;private set; }
        public double DistanceInKiloMeter { get;private set; }
        public string HashedLocation { get;private set; }
        public int? VehicleTypeId { get; private set; }
        public bool IsScheduled { get;private set; }
        public DateTime? ExpectedPickUpTime { get;private set; }
        public int? DeliveryManId { get; private set; }
        public int OrderPackageId { get; private set; }
        public OrderType OrderType { get; private set; }
        public OrderStatus OrderStatus { get; private set; }
        public int VehicleTypdId { get; private set; }
        public decimal Total { get; private set; }
        public decimal TransportAmount { get; private set; }
        public decimal ServiceFee { get; private set; }
        public decimal TaxAmount { get; private set; }
        public bool IsPaid { get; private set; }
        public bool DriverConfirmedGoingToPickup { get; private set; }
        public bool ClientApprovedPickup { get; private set; }
        public DateTime? PickupReminderSentAt { get; private set; }
        public RefundStatus RefundStatus { get; private set; }
        public string? RefundIban { get; private set; }
        public decimal RefundedAmount { get; private set; }
        public int? DiscountCodeId { get; private set; }
        public string? DiscountCodeName { get; private set; }
        public decimal DiscountAmount { get; private set; }
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

        private List<OrderAdditionalService> _OrderAdditionalServices { get; set; }
        public IReadOnlyList<OrderAdditionalService> OrderAdditionalServices
        {
            get
            {
                return _OrderAdditionalServices;
            }
            private set
            {
                _OrderAdditionalServices = (List<OrderAdditionalService>)value.ToList();
            }
        }


        public Result ConfirmOrderWayPointFromCustomer(int orderWayPoint,DateTime nowDate)
        {
            var wayPoint = this._OrderWayPoints
                               .FirstOrDefault(x => x.Id == orderWayPoint);
            if (wayPoint is null)
            {
                return Result.Failure("OrderWayPointNotFound");
            }

            var confirmResult = wayPoint.ChangeStatusToCustomerConfirm(nowDate);
            if (confirmResult.IsFailure)
            {
                return Result.Failure(confirmResult.Error);
            }
            return Result.Success();
        }


        public Result RejectOrderWayPointFromCustomer(int orderWayPoint)
        {
            var wayPoint = this._OrderWayPoints
                               .FirstOrDefault(x => x.Id == orderWayPoint);
            if (wayPoint is null)
            {
                return Result.Failure("OrderWayPointNotFound");
            }

            var confirmResult = wayPoint.ChangeStatusToCustomerReject();
            if (confirmResult.IsFailure)
            {
                return Result.Failure(confirmResult.Error);
            }
            return Result.Success();
        }

        public static Result<Order> Create(int customerId,
                                           OrderType orderType,
                                           string orderNumber,
                                           int orderPackageId,
                                           DateTime nowDate,
                                           List<OrderDetails> orderDetails,
                                           List<OrderWayPoint> orderWayPoints,
                                           List<OrderService> orderServices,
                                           bool isScheduled = false,
                                           DateTime? expectedPickUpTime = null,
                                           List<OrderAdditionalService>? additionalServices = null)
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


            var orderStatusHistory = OrderStatusHistory.Create(OrderStatus.Pending, nowDate);

            var order = new Order
            {
                CustomerId = customerId,
                OrderType = orderType,
                OrderStatus = OrderStatus.Pending,
                OrderDetails = orderDetails,
                OrderWayPoints = orderWayPoints,
                OrderNumber = orderNumber,
                OrderPackageId = 1,
                Total = fixedTotal,
                OrderServices = orderServices,
                OrderAdditionalServices = additionalServices ?? new List<OrderAdditionalService>(),
                IsScheduled = isScheduled,
                ExpectedPickUpTime = expectedPickUpTime,
                CreationDate=nowDate
            };

            order._OrderStatusHistories.Add(orderStatusHistory);
            return order;

        }

        public Result AssignToDeliveryMan(int deliveryManId, DateTime nowDate)
        {
            if (this.OrderStatus != OrderStatus.Pending)
            {
                return Result.Failure("OrderNotInPendingStatus");
            }

            if (this.DeliveryManId != null)
            {
                return Result.Failure("OrderAlreadyAssignedToDeliveryMan");
            }

            if (deliveryManId <= 0)
            {
                return Result.Failure("InvalidDeliveryManId");
            }

            // Update the order status and assign the delivery man
            this.DeliveryManId = deliveryManId;
            this.OrderStatus = OrderStatus.Assigned;

            // Add status history for the assignment
            var statusHistory = OrderStatusHistory.Create(OrderStatus.Assigned, nowDate);
            this._OrderStatusHistories.Add(statusHistory);

            return Result.Success();
        }


        public Result CompleteOrderAndSetVehicleType(int vehicleTypeId,
                                                     decimal vehicleCost,
                                                     decimal customerWalletBalance,
                                                     int baseKm,
                                                     decimal baseKmRate,
                                                     decimal extraKmRate,
                                                     int baseHour,
                                                     decimal baseHourRate,
                                                     decimal extraHourRate,
                                                     decimal vatRate,
                                                     decimal servicesFees,
                                                     int paymentMethodId,
                                                     double distanceInKiloMeter,
                                                     double timeInMinutes,
                                                     string hasedLocation)
        {
            var pricePerKiloMeter= distanceInKiloMeter <= baseKm
                ? baseKmRate
                : extraKmRate;

            var timeInHours=timeInMinutes/ 60.0;

            var pricePerHour = timeInHours <= baseHour
                ? baseHourRate
                : extraHourRate;

            var distanceCost = (decimal)distanceInKiloMeter * pricePerKiloMeter;
            var timeCost = (decimal)timeInHours * pricePerHour;

            var orderServicesSubTotal = this.OrderServices.Select(x => x.Amount).DefaultIfEmpty(0).Sum();

            var additionalServicesSubTotal = this.OrderAdditionalServices.Select(x => x.Amount).DefaultIfEmpty(0).Sum();

            var vatAmount = (distanceCost + timeCost + servicesFees + orderServicesSubTotal + additionalServicesSubTotal) * vatRate / 100.0m;

            var orderTotal = distanceCost + timeCost + servicesFees + orderServicesSubTotal + additionalServicesSubTotal + vatAmount;

            if (customerWalletBalance < orderTotal &&
                paymentMethodId == (int)PaymentMethodEnum.Wallet)
            {
                return Result.Failure("InsufficientWalletBalance");
            }

            this.VehicleTypdId = vehicleTypeId;
            this.DistanceInKiloMeter= distanceInKiloMeter;
            this.ExpectedTimeMinutes= timeInMinutes;
            this.HashedLocation= hasedLocation;
            this.Total = orderTotal;
            this.TransportAmount = distanceCost + timeCost;
            this.ServiceFee = servicesFees;
            this.TaxAmount = vatAmount;

            var orderPaymentMethod = OrderPaymentMethod.Instance(paymentMethodId, orderTotal);
            this._PaymentMethods.Add(orderPaymentMethod);
            return Result.Success();
        }

        public Result UpdateStatus(OrderStatus newStatus, DateTime nowDate)
        {
            if (this.OrderStatus == newStatus)
            {
                return Result.Failure("OrderAlreadyInStatus");
            }

            // Add business rules for status transitions if needed
            if (!IsValidStatusTransition(this.OrderStatus, newStatus))
            {
                return Result.Failure("OrderStatusTransitionNotAllowed");
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
                return Result.Failure("InvalidVehicleTypeId");
            }

            if (this.VehicleTypeId != null)
            {
                return Result.Failure("VehicleTypeAlreadySelected");
            }

            if (this.OrderStatus != OrderStatus.Pending)
            {
                return Result.Failure("VehicleTypeOnlyForPendingOrders");
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

        public Result ApplyDiscount(int discountCodeId, string discountCodeName, decimal discountAmount)
        {
            if (DiscountCodeId.HasValue)
                return Result.Failure("A discount code has already been applied to this order.");

            if (discountAmount < 0)
                return Result.Failure("Discount amount cannot be negative.");

            if (discountAmount > Total)
                return Result.Failure("Discount amount cannot exceed the order total.");

            DiscountCodeId = discountCodeId;
            DiscountCodeName = discountCodeName;
            DiscountAmount = discountAmount;
            Total = Total - discountAmount;

            return Result.Success();
        }

        public Result CancelOrder(DateTime nowDate)
        {
            if (this.OrderStatus == OrderStatus.Cancelled)
            {
                return Result.Failure("OrderAlreadyCancelled");
            }

            this.OrderStatus = OrderStatus.Cancelled;

            // Add status history for cancellation
            var statusHistory = OrderStatusHistory.Create(OrderStatus.Cancelled, nowDate);
            this._OrderStatusHistories.Add(statusHistory);

            return Result.Success();
        }

        public Result ConfirmGoingToPickup(DateTime nowDate)
        {
            if (this.DeliveryManId is null || this.OrderStatus != OrderStatus.Assigned)
            {
                return Result.Failure("OrderMustBeAssignedToConfirmGoingToPickup");
            }

            this.DriverConfirmedGoingToPickup = true;
            this.OrderStatus = OrderStatus.ConfirmedGoingToPickup;
            this._OrderStatusHistories.Add(OrderStatusHistory.Create(OrderStatus.ConfirmedGoingToPickup, nowDate));
            return Result.Success();
        }

        public Result ConfirmPickupByDriver(DateTime nowDate)
        {
            if (this.OrderStatus != OrderStatus.Assigned && this.OrderStatus != OrderStatus.ConfirmedGoingToPickup)
            {
                return Result.Failure("OrderMustBeAssignedOrConfirmedToPickup");
            }

            this.OrderStatus = OrderStatus.PickedUpFromDeliveryMan;
            this._OrderStatusHistories.Add(OrderStatusHistory.Create(OrderStatus.PickedUpFromDeliveryMan, nowDate));
            return Result.Success();
        }

        public Result CancelOrderByDriver(DateTime nowDate)
        {
            if (this.OrderStatus == OrderStatus.Cancelled)
            {
                return Result.Failure("OrderAlreadyCancelled");
            }

            if (this.OrderStatus == OrderStatus.Completed)
            {
                return Result.Failure("CannotCancelCompletedOrder");
            }

            // Driver cancel = reassign: return to pending for manual/admin reassignment (not a client cancellation).
            this.DeliveryManId = null;
            this.DriverConfirmedGoingToPickup = false;
            this.OrderStatus = OrderStatus.Pending;
            this._OrderStatusHistories.Add(OrderStatusHistory.Create(OrderStatus.Pending, nowDate));
            return Result.Success();
        }

        public void ApproveClientPickup()
        {
            this.ClientApprovedPickup = true;
        }

        public void MarkPickupReminderSent(DateTime nowDate)
        {
            this.PickupReminderSentAt = nowDate;
        }

        public Result MarkAsPaid()
        {
            if (this.IsPaid)
            {
                return Result.Failure("OrderAlreadyPaid");
            }

            this.IsPaid = true;
            return Result.Success();
        }

        public Result InitiateRefund(string iban)
        {
            if (!this.IsPaid)
            {
                return Result.Failure("OrderIsNotPaidNoRefundNeeded");
            }

            if (string.IsNullOrWhiteSpace(iban))
            {
                return Result.Failure("IbanIsRequiredToRefundAPaidOrder");
            }

            if (this.RefundStatus != RefundStatus.None)
            {
                return Result.Failure("RefundAlreadyInitiated");
            }

            this.RefundIban = iban.Trim();
            this.RefundedAmount = this.Total;
            this.RefundStatus = RefundStatus.Pending;
            return Result.Success();
        }
    }
}
