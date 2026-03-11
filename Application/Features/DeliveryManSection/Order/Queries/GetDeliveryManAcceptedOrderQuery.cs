using Application.Features.DeliveryManSection.Order.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Order.Queries
{
    public sealed record GetDeliveryManAcceptedOrderQuery : IRequest<Result<List<AcceptedOrder>>>
    {
        private class GetDeliveryManAcceptedOrderQueryHandler : IRequestHandler<GetDeliveryManAcceptedOrderQuery, Result<List<AcceptedOrder>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            private readonly IReadFromAppSetting config;
            private const string DeliveryOrderFolderPrefix = "DeliveryOrders";

            public GetDeliveryManAcceptedOrderQueryHandler(INaqlahContext context,
                                                           IUserSession userSession,
                                                           IReadFromAppSetting config)
            {
                this.context = context;
                this.userSession = userSession;
                this.config = config;
            }
            public async Task<Result<List<AcceptedOrder>>> Handle(GetDeliveryManAcceptedOrderQuery request, CancellationToken cancellationToken)
            {
                var deliveryMan = await context.DeliveryMen
                                             .FirstOrDefaultAsync(x => x.UserId == userSession.UserId,
                                                                  cancellationToken);

                if (deliveryMan == null) 
                {
                    return Result.Failure<List<AcceptedOrder>>("Delivery Man Not Found");
                }

                var orders = await context.Orders
                                        .Include(x => x.OrderDetails)
                                        .Include(x => x.OrderWayPoints)
                                        .Include(x => x.OrderServices)
                                        .Include(x => x.PaymentMethods)
                                        .Include(x => x.OrderPackage)
                                        .Where(x => x.DeliveryManId == deliveryMan.Id && x.OrderStatus == Domain.Enums.OrderStatus.Assigned)
                                        .ToListAsync(cancellationToken);

                if (!orders.Any())
                {
                    return new List<AcceptedOrder>();
                }

                var customers=await context.Customers
                                         .Where(c => orders.Select(o => o.CustomerId).Contains(c.Id))
                                         .ToListAsync(cancellationToken);

              

                var regions=await context.Regions
                                        .Include(x=>x.Cities)
                                         .ThenInclude(x=>x.Neighborhoods)
                                        .ToListAsync(cancellationToken);



                var baseUrl = config.GetValue<string>("apiBaseUrl");

                var acceptedOrders = orders.Select(order => new AcceptedOrder
                {
                    OrderId = order.Id,
                    IsScheduled=order.IsScheduled,
                    ExpectedPickUpTime=order.ExpectedPickUpTime,
                    OrderNumber = order.OrderNumber,
                    CustomerPhone = customers.Where(x=>x.Id==order.CustomerId)
                                             .Select(x=>x.PhoneNumber)
                                             .FirstOrDefault()??"",
                    CustomerName = customers.Where(x => x.Id == order.CustomerId)
                                             .Select(x => x.PhoneNumber)
                                             .FirstOrDefault() ?? "",
                    CustomerNotes = "",
                    OrderStatus = (int)order.OrderStatus,
                    OrderType = (int)order.OrderType,
                    Total = order.Total,
                    CustomerId = order.CustomerId,
                    WayPoints = order.OrderWayPoints.Select(wp => new WayPoint
                    {
                        Id = wp.Id,
                        Longitude=wp.longitude,
                        Latitude=wp.Latitude,
                        BackAndForth=false,
                        IsOrigin=wp.IsOrgin,
                        IsDestination=wp.IsDestination,
                        PickedUpDate=wp.PickedUpDate?.ToString("o")??"",
                        Status=(int)wp.OrderWayPointsStatus,
                        CityArabicName=regions
                                            .SelectMany(r=>r.Cities)
                                            .Where(c=>c.Id==wp.CityId)
                                            .Select(c=>c.ArabicName)
                                            .FirstOrDefault()??"",
                        CityEnglishName= regions
                                            .SelectMany(r => r.Cities)
                                            .Where(c => c.Id == wp.CityId)
                                            .Select(c => c.EnglishName)
                                            .FirstOrDefault() ?? "",
                        NeighborhoodArabicName= regions
                                                    .SelectMany(r => r.Cities)
                                                    .SelectMany(c=>c.Neighborhoods)
                                                    .Where(n=>n.Id==wp.NeighborhoodId)
                                                    .Select(n=>n.ArabicName)
                                                    .FirstOrDefault()??"",
                        NeighborhoodEnglishName= regions
                                                    .SelectMany(r => r.Cities)
                                                    .SelectMany(c => c.Neighborhoods)
                                                    .Where(n => n.Id == wp.NeighborhoodId)
                                                    .Select(n => n.EnglishName)
                                                    .FirstOrDefault() ?? "",
                        RegionArabicName= regions
                                            .Where(r=>r.Id==wp.RegionId)
                                            .Select(r=>r.ArabicName)
                                            .FirstOrDefault()??"",
                        RegionEnglishName= regions
                                            .Where(r => r.Id == wp.RegionId)
                                            .Select(r => r.EnglishName)
                                            .FirstOrDefault() ?? "",
                        PackImagePath= $"{baseUrl}/ImageBank/{DeliveryOrderFolderPrefix}/Order_{order.Id}/{wp.PackImagePath}"

                    }).ToList(),
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetail
                    {
                        Id = od.Id,
                        ArabicCategoryName= od.ArabicCategoryName,
                        EnglishCategoryName= od.EnglishCategoryName,
                        MainCategoryId= od.MainCategoryId,
                    }).ToList(),
                    OrderServices = order.OrderServices.Select(os => new OrderService
                    {
                        Id = os.Id,
                        ArabicName= os.ArabicName,
                        EnglishName= os.EnglishName,
                        Amount= os.Amount,
                        WorkId= os.WorkId,
                    }).ToList(),
                    OrderPackageArabicDescription = order.OrderPackage?.ArabicDescripton??"",
                    OrderPackageEnglishDescription = order.OrderPackage?.EnglishDescription??"",
                    PaymentMethods = order.PaymentMethods.Select(pm => new DeliveryPaymentMethodDto
                    {
                        PaymentMethodId=pm.PaymentMethodId,
                        PaymentMethodArabicName= "محفظة إلكترونية",
                        PaymentMethodEnglishName="Wallet",
                        Amount=pm.Amount
                    }).ToList()
                }).ToList();

                return acceptedOrders;

            }
        }
    }
}
