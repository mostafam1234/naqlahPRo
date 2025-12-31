using Application.Features.AdminSection.Dashboard.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.Dashboard.Queries
{
    public sealed record GetTodayOrdersQuery(int LanguageId) : IRequest<Result<List<TodayOrderDto>>>
    {
        private class GetTodayOrdersQueryHandler : IRequestHandler<GetTodayOrdersQuery, Result<List<TodayOrderDto>>>
        {
            private readonly INaqlahContext _context;

            public GetTodayOrdersQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<List<TodayOrderDto>>> Handle(GetTodayOrdersQuery request, CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // Get today's orders with VehicleType
                var todayOrders = await (from order in _context.Orders
                                        join vehicleType in _context.VehicleTypes on order.VehicleTypeId equals vehicleType.Id into vt
                                        from vehicleType in vt.DefaultIfEmpty()
                                        where order.CreationDate >= today && order.CreationDate < tomorrow
                                        orderby order.CreationDate descending
                                        select new TodayOrderDto
                                        {
                                            Id = order.Id,
                                            VehicleTypeName = vehicleType != null 
                                                ? (isArabic ? vehicleType.ArabicName : vehicleType.EnglishName)
                                                : "غير محدد",
                                            OrderType = order.OrderType,
                                            OrderTypeName = GetOrderTypeName(order.OrderType, request.LanguageId),
                                            Total = order.Total,
                                            OrderStatus = order.OrderStatus,
                                            OrderStatusName = GetOrderStatusName(order.OrderStatus, request.LanguageId)
                                        })
                                        .ToListAsync(cancellationToken);

                return Result.Success(todayOrders);
            }

            private static string GetOrderTypeName(OrderType orderType, int languageId)
            {
                return orderType switch
                {
                    OrderType.SingleWayPoints => languageId == 1 ? "نقطة واحدة" : "Single Way Point",
                    OrderType.MultiWayPoints => languageId == 1 ? "عدة نقاط" : "Multiple Way Points",
                    OrderType.BackAndForth => languageId == 1 ? "ذهاب وعودة" : "Back and Forth",
                    _ => languageId == 1 ? "غير محدد" : "Not Specified"
                };
            }

            private static string GetOrderStatusName(OrderStatus status, int languageId)
            {
                return status switch
                {
                    OrderStatus.Pending => languageId == 1 ? "في الانتظار" : "Pending",
                    OrderStatus.Assigned => languageId == 1 ? "تم تعيين كابتن" : "Assigned",
                    OrderStatus.Completed => languageId == 1 ? "مكتمل" : "Completed",
                    OrderStatus.Cancelled => languageId == 1 ? "ملغي" : "Cancelled",
                    _ => languageId == 1 ? "غير محدد" : "Not Specified"
                };
            }
        }
    }
}

