using Application.Features.AdminSection.OrderFeature.Commands;
using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Features.AdminSection.OrderFeature.Queries;
using Application.Shared.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Reponse;
using Domain.InterFaces;
using Presentaion.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Presentaion.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderAdminController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;
        private readonly NotificationHubService notificationHubService;

        public OrderAdminController(IMediator mediator, IUserSession userSession, NotificationHubService notificationHubService)
        {
            this.mediator = mediator;
            this.userSession = userSession;
            this.notificationHubService = notificationHubService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<GetAllOrdersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllOrders")]
        public async Task<IActionResult> GetAllOrders(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] OrderStatus? statusFilter = null,
        [FromQuery] CustomerType? customerTypeFilter = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
        {
            var query = new GetAllOrdersQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                CustomerTypeFilter = customerTypeFilter,
                FromDate = fromDate,
                ToDate = toDate,
                LanguageId = this.userSession.LanguageId
            };

            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetOrderDetailsForAdminDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetOrderDetails")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var query = new GetOrderDetailsByOrderIdForAdminQuery
            {
                OrderId = id,
                LanguageId = this.userSession.LanguageId
            };

            var result = await mediator.Send(query);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var command = new CancelOrderFromAdmin
            {
                OrderId = id,
                LanguageId = this.userSession.LanguageId
            };

            var result = await mediator.Send(command);
            if (result.IsSuccess)
            {
                // Send notification for order status change
                try
                {
                    var orderDetailsQuery = new GetOrderDetailsByOrderIdForAdminQuery
                    {
                        OrderId = id,
                        LanguageId = this.userSession.LanguageId
                    };
                    var orderDetailsResult = await mediator.Send(orderDetailsQuery);
                    
                    if (orderDetailsResult.IsSuccess && orderDetailsResult.Value != null)
                    {
                        var orderNumber = orderDetailsResult.Value.OrderNumber ?? id.ToString();
                        await notificationHubService.SendNotificationAsync(
                            arabicTitle: "تغيير حالة الطلب",
                            englishTitle: "Order Status Changed",
                            arabicMessage: $"تم إلغاء الطلب رقم {orderNumber}",
                            englishMessage: $"Order {orderNumber} has been cancelled",
                            notificationType: NotificationType.OrderStatusChanged,
                            orderId: id
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the request
                }

                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AssignOrderToDeliveryMan")]
        public async Task<IActionResult> AssignOrderToDeliveryMan([FromBody] AssignOrderToDeliveryManRequest request)
        {
            var command = new AssignOrderToDeliveryManFromAdmin
            {
                OrderId = request.OrderId,
                DeliveryManId = request.DeliveryManId,
                LanguageId = this.userSession.LanguageId
            };

            var result = await mediator.Send(command);
            if (result.IsSuccess)
            {
                // Send notification for order status change
                try
                {
                    var orderDetailsQuery = new GetOrderDetailsByOrderIdForAdminQuery
                    {
                        OrderId = request.OrderId,
                        LanguageId = this.userSession.LanguageId
                    };
                    var orderDetailsResult = await mediator.Send(orderDetailsQuery);
                    
                    if (orderDetailsResult.IsSuccess && orderDetailsResult.Value != null)
                    {
                        var orderNumber = orderDetailsResult.Value.OrderNumber ?? request.OrderId.ToString();
                        await notificationHubService.SendNotificationAsync(
                            arabicTitle: "تغيير حالة الطلب",
                            englishTitle: "Order Status Changed",
                            arabicMessage: $"تم تعيين مندوب التوصيل للطلب رقم {orderNumber}",
                            englishMessage: $"Delivery man has been assigned to order {orderNumber}",
                            notificationType: NotificationType.OrderStatusChanged,
                            orderId: request.OrderId
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the request
                }

                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("CompleteOrder")]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var command = new CompleteOrderFromAdmin
            {
                OrderId = id,
                LanguageId = this.userSession.LanguageId
            };

            var result = await mediator.Send(command);
            if (result.IsSuccess)
            {
                // Send notification for order status change
                try
                {
                    var orderDetailsQuery = new GetOrderDetailsByOrderIdForAdminQuery
                    {
                        OrderId = id,
                        LanguageId = this.userSession.LanguageId
                    };
                    var orderDetailsResult = await mediator.Send(orderDetailsQuery);
                    
                    if (orderDetailsResult.IsSuccess && orderDetailsResult.Value != null)
                    {
                        var orderNumber = orderDetailsResult.Value.OrderNumber ?? id.ToString();
                        await notificationHubService.SendNotificationAsync(
                            arabicTitle: "تغيير حالة الطلب",
                            englishTitle: "Order Status Changed",
                            arabicMessage: $"تم إكمال الطلب رقم {orderNumber}",
                            englishMessage: $"Order {orderNumber} has been completed",
                            notificationType: NotificationType.OrderStatusChanged,
                            orderId: id
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the request
                }

                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AvailableDeliveryManDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAvailableDeliveryMen")]
        public async Task<IActionResult> GetAvailableDeliveryMen()
        {
            var query = new GetAvailableDeliveryMenForAssignmentQuery
            {
                LanguageId = this.userSession.LanguageId
            };

            var result = await mediator.Send(query);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
    }

    public class AssignOrderToDeliveryManRequest
    {
        public int OrderId { get; set; }
        public int DeliveryManId { get; set; }
    }
}
