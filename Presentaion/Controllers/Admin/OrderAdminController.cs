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
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Presentaion.Authorization;

namespace Presentaion.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequirePermission(PermissionNames.CanViewAllOrders)]
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
        [ProducesResponseType(typeof(OrderStatisticsDto), StatusCodes.Status200OK)]
        [Route("GetOrderStatistics")]
        public async Task<IActionResult> GetOrderStatistics()
        {
            var result = await mediator.Send(new GetOrderStatisticsQuery
            {
                LanguageId = this.userSession.LanguageId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("ExportOrderStatistics")]
        public async Task<IActionResult> ExportOrderStatistics()
        {
            var result = await mediator.Send(new ExportOrderStatisticsToExcelQuery
            {
                LanguageId = this.userSession.LanguageId
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            var exportResult = result.Value;
            return File(exportResult.Stream, exportResult.ContentType, exportResult.FileName);
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
        [FromQuery] bool? activeOrdersOnly = null,
        [FromQuery] CustomerType? customerTypeFilter = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] List<int>? deliveryManIds = null)
        {
            var query = new GetAllOrdersQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                ActiveOrdersOnly = activeOrdersOnly,
                CustomerTypeFilter = customerTypeFilter,
                FromDate = fromDate,
                ToDate = toDate,
                DeliveryManIds = deliveryManIds,
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
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("ExportAllOrders")]
        public async Task<IActionResult> ExportAllOrders(
            [FromQuery] string? searchTerm = null,
            [FromQuery] OrderStatus? statusFilter = null,
            [FromQuery] bool? activeOrdersOnly = null,
            [FromQuery] CustomerType? customerTypeFilter = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] List<int>? deliveryManIds = null)
        {
            var result = await mediator.Send(new ExportAllOrdersToExcelQuery
            {
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                ActiveOrdersOnly = activeOrdersOnly,
                CustomerTypeFilter = customerTypeFilter,
                FromDate = fromDate,
                ToDate = toDate,
                DeliveryManIds = deliveryManIds,
                LanguageId = this.userSession.LanguageId
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            var exportResult = result.Value;
            return File(exportResult.Stream, exportResult.ContentType, exportResult.FileName);
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

        // OrderPackage endpoints
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<OrderPackageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllOrderPackages")]
        public async Task<IActionResult> GetAllOrderPackages(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null)
        {
            var query = new GetAllOrderPackagesQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm
            };

            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AddOrderPackage")]
        public async Task<IActionResult> AddOrderPackage([FromBody] AddOrderPackageCommand command)
        {
            var result = await mediator.Send(new AddOrderPackageCommand
            {
                ArabicDescription = command.ArabicDescription,
                EnglishDescription = command.EnglishDescription,
                MinWeightInKiloGram = command.MinWeightInKiloGram,
                MaxWeightInKiloGram = command.MaxWeightInKiloGram
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("UpdateOrderPackage")]
        public async Task<IActionResult> UpdateOrderPackage([FromBody] UpdateOrderPackageCommand command)
        {
            var result = await mediator.Send(new UpdateOrderPackageCommand
            {
                Id = command.Id,
                ArabicDescription = command.ArabicDescription,
                EnglishDescription = command.EnglishDescription,
                MinWeightInKiloGram = command.MinWeightInKiloGram,
                MaxWeightInKiloGram = command.MaxWeightInKiloGram
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("DeleteOrderPackage")]
        public async Task<IActionResult> DeleteOrderPackage([FromQuery] int orderPackageId)
        {
            var result = await mediator.Send(new DeleteOrderPackageCommand
            {
                Id = orderPackageId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<OrderRatingAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetOrderRatings")]
        public async Task<IActionResult> GetOrderRatings(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? minRating = null,
            [FromQuery] int? maxRating = null)
        {
            var result = await mediator.Send(new GetOrderRatingsQuery
            {
                Skip = skip,
                Take = take,
                FromDate = fromDate,
                ToDate = toDate,
                MinRating = minRating,
                MaxRating = maxRating
            });

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            return Ok(result.Value);
        }
    }

    public class AssignOrderToDeliveryManRequest
    {
        public int OrderId { get; set; }
        public int DeliveryManId { get; set; }
    }
}
