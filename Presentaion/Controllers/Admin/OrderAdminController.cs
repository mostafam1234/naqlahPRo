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

        public OrderAdminController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
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
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
    }
}
