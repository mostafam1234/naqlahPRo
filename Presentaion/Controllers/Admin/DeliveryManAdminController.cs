using Application.Features.AdminSection.DeliveryManFeature.Dtos;
using Application.Features.AdminSection.DeliveryManFeature.Queries;
using Application.Features.DeliveryManSection.Assistant.Dtos;
using Application.Features.DeliveryManSection.Assistant.Queries;
using Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands;
using Application.Features.DeliveryManSection.CurrentDeliveryMen.Dtos;
using Application.Features.DeliveryManSection.CurrentDeliveryMen.Queries;
using Application.Features.DeliveryManSection.NewRequests.Commands;
using Application.Features.DeliveryManSection.NewRequests.Dtos;
using Application.Features.DeliveryManSection.NewRequests.Queries;
using Application.Shared.Dtos;
using Domain.InterFaces;
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

namespace Presentaion.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveryManAdminController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;

        public DeliveryManAdminController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedGetAllDeliveryMenRequestsPaged), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllDeliveryMenRequests")]
        public async Task<IActionResult> GetAllDeliveryMenRequests(int skip, int take, int deliveryTypeFilter = 0, string searchTerm = "")
        {
            var result = await mediator.Send(new GetAllDeliveryMenRequestsQuery
            {
                Skip = skip,
                Take = take,
                DeliveryTypeFilter = deliveryTypeFilter,
                SearchTerm = searchTerm ?? string.Empty
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetDeliveryManRequestDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetDeliveryManDetails/{deliveryManId}")]
        public async Task<IActionResult> GetDeliveryManDetails(int deliveryManId)
        {
            var result = await mediator.Send(new GetDeliveryMenRequestsByDeliveryManIdQuery
            {
                DeliveryManId = deliveryManId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("UpdateDeliveryManState")]

        public async Task<IActionResult> UpdateDeliveryManState(int deliveryManId, int state)
        {
            var result = await mediator.Send(new UpdateDeliveryManState
            {
                DeliveryId = deliveryManId,
                State = state
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedGetAllApprovedDeliveryMenPaged), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllApprovedDeliveryMen")]
        public async Task<IActionResult> GetAllApprovedDeliveryMen(int skip = 0, int take = 10, int deliveryTypeFilter = 0, string searchTerm = "")
        {
            var result = await mediator.Send(new GetAllApprovedDeliveryMenQuery
            {
                Skip = skip,
                Take = take,
                DeliveryTypeFilter = deliveryTypeFilter,
                SearchTerm = searchTerm ?? string.Empty
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AddDeliveryMan")]
        public async Task<IActionResult> AddDeliveryMan([FromBody] AddDeliveryManDto deliveryManDto)
        {
            var result = await mediator.Send(new AddDeliveryManCommand
            {
                DeliveryMan = deliveryManDto
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("UpdateDeliveryMan/{deliveryManId}")]
        public async Task<IActionResult> UpdateDeliveryMan(int deliveryManId, [FromBody] AddDeliveryManDto deliveryManDto)
        {
            var result = await mediator.Send(new UpdateDeliveryManCommand
            {
                DeliveryManId = deliveryManId,
                DeliveryMan = deliveryManDto
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<GetAllDeliveryMenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllDeliveryMen")]
        public async Task<IActionResult> GetAllDeliveryMen(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null)
        {
            var query = new GetAllDeliveryMenQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                LanguageId = this.userSession.LanguageId
            };

            var result = await mediator.Send(query);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(DeliveryManStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetDeliveryManStatistics")]
        public async Task<IActionResult> GetDeliveryManStatistics()
        {
            var query = new GetDeliveryManStatisticsQuery();

            var result = await mediator.Send(query);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<Application.Features.AdminSection.OrderFeature.Dtos.GetAllOrdersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetOrdersByDeliveryManId")]
        public async Task<IActionResult> GetOrdersByDeliveryManId(
            [FromQuery] int deliveryManId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Domain.Enums.OrderStatus? statusFilter = null)
        {
            var query = new GetOrdersByDeliveryManIdQuery
            {
                DeliveryManId = deliveryManId,
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                LanguageId = this.userSession.LanguageId
            };

            var result = await mediator.Send(query);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }
    }
}
