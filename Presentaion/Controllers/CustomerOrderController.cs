using Application.Features.AdminSection.OrderFeature.Queries;
using Application.Features.CustomerSection.Feature.AssistantWork.Dtos;
using Application.Features.CustomerSection.Feature.AssistantWork.Queries;
using Application.Features.CustomerSection.Feature.Order.Commands;
using Application.Features.CustomerSection.Feature.Order.Dtos;
using Application.Features.CustomerSection.Feature.Order.Queries;
using Application.Features.CustomerSection.Feature.Regestration.Commands;
using Application.Features.CustomerSection.Feature.Regestration.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Reponse;
using Presentaion.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentaion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerOrderController:ControllerBase
    {
        private readonly IMediator mediator;
        private readonly NotificationHubService notificationHubService;

        public CustomerOrderController(IMediator mediator, NotificationHubService notificationHubService)
        {
            this.mediator = mediator;
            this.notificationHubService = notificationHubService;
        }


        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderResponseDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto request)
        {
            var result = await mediator.Send(new CreateNewOrderCommand
            {
                Order=request,
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            // Send notification for new order
            try
            {
                var orderNumber = result.Value.OrderNumber ?? result.Value.OrderId.ToString();
                await notificationHubService.SendNotificationAsync(
                    arabicTitle: "طلب جديد",
                    englishTitle: "New Order",
                    arabicMessage: $"تم إنشاء طلب جديد برقم {orderNumber}",
                    englishMessage: $"A new order has been created with number {orderNumber}",
                    notificationType: NotificationType.NewOrder,
                    orderId: result.Value.OrderId
                );
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                // TODO: Add logging here
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SelectVehicleTypeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("SelectVehicleType")]
        public async Task<IActionResult> SelectVehicleType([FromBody] SelectVehicleTypeDto request)
        {
            var result = await mediator.Send(new SelectVehicleTypeForOrderCommand(request));

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedCustomerOrdersDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetMyOrders")]
        public async Task<IActionResult> GetMyOrders([FromQuery] CustomerOrdersQueryRequest request)
        {
            var result = await mediator.Send(new GetCustomerOrdersQuery(request));

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(OrderDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetOrderDetails/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var result = await mediator.Send(new GetOrderDetailsByIdQuery(orderId));

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AssistantWorkDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAssistantWorks")]
        public async Task<IActionResult> GetAssistantWorks()
        {
            var result = await mediator.Send(new GetAssistantWorksQuery());

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("CheckPendingOrder")]
        public async Task<IActionResult> CheckPendingOrder()
        {
            var result = await mediator.Send(new CheckPendingOrderQuery());

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

    }
}
