using Application.Features.DeliveryManSection.Order.Commands;
using Application.Features.DeliveryManSection.Order.Dtos;
using Application.Features.DeliveryManSection.Order.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Reponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentaion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for delivery man endpoints
    public class DeliveryOrderController : ControllerBase
    {
        private readonly IMediator mediator;

        public DeliveryOrderController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PendingOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetPendingOrdersWithinRadius")]
        public async Task<IActionResult> GetPendingOrdersWithinRadius()
        {
            var result = await mediator.Send(new GetPendingOrdersWithinRadiusQuery());

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AssignOrder")]
        public async Task<IActionResult> AssignOrder([FromBody] AssignOrderRequestDto request)
        {
            var result = await mediator.Send(new AssignOrderToDeliveryManCommand
            {
                Request = request
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(OrderDetailsResponse), StatusCodes.Status200OK)]
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("ChangeWayPointStatus")]
        public async Task<IActionResult> ChangeWayPointStatus([FromBody] ChangeWayPointStatusRequestDto request)
        {
            var result = await mediator.Send(new ChangeOrderWayPointStatusCommand
            {
                OrderId = request.OrderId,
                WayPointId = request.WayPointId,
                PackImageBase64 = request.PackImageBase64
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok();
        }
    }
}