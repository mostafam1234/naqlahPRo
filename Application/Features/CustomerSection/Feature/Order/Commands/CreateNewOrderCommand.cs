using Application.Features.CustomerSection.Feature.Order.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Application.Features.CustomerSection.Feature.Order.Commands
{
    public sealed record CreateNewOrderCommand:IRequest<Result<CreateOrderResponseDto>>
    {
        public CreateOrderDto Order { get; init; }

        private class CreateNewOrderCommandHandler : IRequestHandler<CreateNewOrderCommand,
                                                                      Result<CreateOrderResponseDto>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            private readonly IDateTimeProvider dateTimeProvider;
            public const int WalletPaymentMethodId = (int)PaymentMethodEnum.Wallet;
            public CreateNewOrderCommandHandler(INaqlahContext context,
                                                IUserSession userSession,
                                                IDateTimeProvider dateTimeProvider)
            {
                this.context = context;
                this.userSession = userSession;
                this.dateTimeProvider = dateTimeProvider;
            }
            public async Task<Result<CreateOrderResponseDto>> Handle(CreateNewOrderCommand request,
                                                                     CancellationToken cancellationToken)
            {
                var nowDate = dateTimeProvider.Now;
                var orderDetailsResult =await BuildOrderDetails(request.Order.MainCategoryIds);
                if (orderDetailsResult.IsFailure)
                {
                    return Result.Failure<CreateOrderResponseDto>(orderDetailsResult.Error);
                }
                var orderWayPointsResult = BuildOrderWayPoints(request.Order.WayPoints);
                if (orderWayPointsResult.IsFailure)
                {
                    return Result.Failure<CreateOrderResponseDto>(orderWayPointsResult.Error);
                }

                DateTime expectedPickUpTIme = DateTime.UtcNow ;

                if (request.Order.IsScheduled && !DateTime.TryParseExact(
                   request.Order.ExpectedPickUpTime,
                    "yyyy/MM/dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out  expectedPickUpTIme))
                {
                    return Result.Failure<CreateOrderResponseDto>("InValid format of expected pickUp Tiem yyyy/MM/dd HH:mm:ss");
                }



                var customerId = await context.Customers
                                          .Where(x=>x.UserId==userSession.UserId)
                                          .Select(x => x.Id)
                                          .FirstOrDefaultAsync();
                if (customerId == 0)
                {
                    return Result.Failure<CreateOrderResponseDto>("Customer not found");
                }

                //// Check if customer has any pending orders
                //var hasPendingOrder = await context.Orders
                //    .AnyAsync(o => o.CustomerId == customerId && o.OrderStatus == OrderStatus.Pending, cancellationToken);
                
                //if (hasPendingOrder)
                //{
                //    return Result.Failure<CreateOrderResponseDto>("Cannot create new order. Customer has a pending order that needs to be completed first.");
                //}

                var orderServices = await BuildOrderService(request.Order.OrderServiceIds);

                var additionalServicesResult = await BuildOrderAdditionalServices(request.Order.AdditionalServices);
                if (additionalServicesResult.IsFailure)
                {
                    return Result.Failure<CreateOrderResponseDto>(additionalServicesResult.Error);
                }

                var orderNumber = await GenerateUniqueOrderNumberAsync();
                DateTime? expectedPickUpTimeValue=null;

                if (request.Order.IsScheduled)
                {
                    expectedPickUpTimeValue = expectedPickUpTIme;
                }

                var orderResult = Domain.Models.Order.Create(customerId: customerId,
                                              (OrderType)request.Order.OrderTypeId,
                                              orderNumber: orderNumber,
                                              orderPackageId: request.Order.OrderPackId,
                                              nowDate: nowDate,
                                              orderDetailsResult.Value,
                                              orderWayPointsResult.Value,
                                              orderServices,
                                              request.Order.IsScheduled,
                                              expectedPickUpTimeValue,
                                              additionalServicesResult.Value);
                
                if (orderResult.IsFailure)
                {
                    return Result.Failure<CreateOrderResponseDto>(orderResult.Error);
                }

                // Save the order to database
                context.Orders.Add(orderResult.Value);
                var saveResult = await context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                {
                    return Result.Failure<CreateOrderResponseDto>(saveResult.Error);
                }

                var systemConfig = await context.SystemConfigurations.FirstOrDefaultAsync(cancellationToken);

                var additionalServicesTotal = additionalServicesResult.Value
                    .Select(x => x.Amount).DefaultIfEmpty(0).Sum();

                // Get matching vehicles based on selected categories
                var matchingVehicles = await GetMatchingVehicles(request.Order.MainCategoryIds,
                                                                 systemConfig,
                                                                 additionalServicesTotal);

                var response = new CreateOrderResponseDto
                {
                    OrderId = orderResult.Value.Id,
                    OrderNumber = orderResult.Value.OrderNumber,
                    MatchingVehicles = matchingVehicles
                };

                return Result.Success(response);
            }

            private async Task<Result<List<OrderDetails>>> BuildOrderDetails(List<int> mainCategoryIds)
            {
                if (mainCategoryIds.Count == 0)
                {
                    return Result.Failure<List<OrderDetails>>("At least one main category is required");
                }

                var categories =await context.MainCategories
                                        .Where(mc => mainCategoryIds.Contains(mc.Id))
                                        .ToListAsync();

                var orderDetails = categories.Select(mc => OrderDetails.Create(mc.Id,
                                                                             mc.ArabicName,
                                                                 mc.EnglishName)).ToList();

                return Result.Success(orderDetails);
            }


            private Result<List<OrderWayPoint>> BuildOrderWayPoints(List<CreateWayPointsDto> orderWayPointDtos)
            {
                if (orderWayPointDtos.Count == 0)
                {
                    return Result.Failure<List<OrderWayPoint>>("At least two way points are required");
                }
                var wayPoines = orderWayPointDtos.Select(wp => OrderWayPoint.Create(wp.Latitude,
                                                                           wp.Longitude,
                                                                           wp.RegionId,
                                                                           wp.CityId,
                                                                           wp.NeighborhoodId,
                                                                           wp.IsOrgin,
                                                                           wp.IsDestenation)).ToList();

                return Result.Success(wayPoines);
            }


            public async Task<List<OrderService>> BuildOrderService(List<int> orderServiceIds)
            {
                if (orderServiceIds.Count == 0)
                {
                    return new List<OrderService>();
                }

                var works=await context.AssistanWorks
                    .Where(x=>orderServiceIds.Contains(x.Id))
                    .ToListAsync();

                var orderServices = works.Select(w => OrderService.Instance(w.Id,
                                                                          w.Cost,
                                                                         w.ArabicName,
                                                                         w.EnglishName)).ToList();

                return orderServices;
            }

            public async Task<Result<List<Domain.Models.OrderAdditionalService>>> BuildOrderAdditionalServices(
                List<AdditionalServiceItemDto> items)
            {
                if (items == null || items.Count == 0)
                    return Result.Success(new List<Domain.Models.OrderAdditionalService>());

                var serviceIds = items.Select(x => x.ServiceId).ToList();
                var catalog = await context.AdditionalServices
                    .Where(x => serviceIds.Contains(x.Id) && !x.IsDeleted)
                    .ToListAsync();

                var result = new List<Domain.Models.OrderAdditionalService>();
                foreach (var item in items)
                {
                    var service = catalog.FirstOrDefault(x => x.Id == item.ServiceId);
                    if (service is null)
                        return Result.Failure<List<Domain.Models.OrderAdditionalService>>(
                            $"Additional service with id {item.ServiceId} not found");

                    var createResult = Domain.Models.OrderAdditionalService.Create(service.Id,
                                                                                   item.Quantity,
                                                                                   service.UnitPrice);
                    if (createResult.IsFailure)
                        return Result.Failure<List<Domain.Models.OrderAdditionalService>>(createResult.Error);

                    result.Add(createResult.Value);
                }

                return Result.Success(result);
            }

            public async Task<string> GenerateUniqueOrderNumberAsync()
            {
                string orderNumber;
                bool exists;

                do
                {
                    orderNumber = GenerateOrderNumber();
                    exists = await context.Orders
                        .AnyAsync(o => o.OrderNumber == orderNumber);
                }
                while (exists);

                return orderNumber;
            }

            private static string GenerateOrderNumber()
            {
                var random = new Random();
                return random.Next(100000, 999999).ToString();
            }

            private async Task<List<OrderVehicleDto>> GetMatchingVehicles(List<int> mainCategoryIds,
                                                                            Domain.Models.SystemConfiguration? systemConfig,
                                                                            decimal additionalServicesTotal)
            {
                var languageId = userSession.LanguageId;

                var serviceFee = systemConfig?.ServiceFess ?? 0;
                var vatRate = systemConfig?.VatRate ?? 0;

                var vehicleTypes = await context.VehicleTypes
                    .Include(vt => vt.VehicleTypeCategoies)
                    .Where(vt => mainCategoryIds.All(categoryId =>
                        vt.VehicleTypeCategoies.Any(vtc => vtc.MainCategoryId == categoryId)))
                    .Select(vt => new
                    {
                        vt.Id,
                        Name = languageId == (int)Domain.Enums.Language.Arabic ?
                               vt.ArabicName : vt.EnglishName,
                        IconPath = "https://naqlah.runasp.net/ImageBank/VehicleType/bigOne.png"
                    })
                    .ToListAsync();

                return vehicleTypes.Select(vt =>
                {
                    var taxAmount = (serviceFee + additionalServicesTotal) * vatRate / 100m;
                    var total = serviceFee + additionalServicesTotal + taxAmount;
                    return new OrderVehicleDto
                    {
                        Id = vt.Id,
                        Name = vt.Name,
                        IconPath = vt.IconPath,
                        Price = total,
                        TotalPrice = total,
                        BasePrice = 0,
                        TransportAmount = 0,
                        ServiceFee = serviceFee,
                        TaxAmount = taxAmount,
                        DiscountAmount = 0,
                        AdditionalServicesTotal = additionalServicesTotal
                    };
                }).ToList();
            }
        }
    }
}
