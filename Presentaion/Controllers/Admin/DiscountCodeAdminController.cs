using Application.Features.AdminSection.DiscountCodeFeatures.Commands;
using Application.Features.AdminSection.DiscountCodeFeatures.Dtos;
using Application.Features.AdminSection.DiscountCodeFeatures.Queries;
using Application.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Reponse;

namespace Presentaion.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DiscountCodeAdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DiscountCodeAdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("GetAllDiscountCodes")]
        [ProducesResponseType(typeof(PagedResult<DiscountCodeAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllDiscountCodes(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            var result = await _mediator.Send(new GetAllDiscountCodesQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                IsActive = isActive
            });

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            return Ok(result.Value);
        }

        [HttpPost]
        [Route("AddDiscountCode")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDiscountCode([FromBody] AddDiscountCodeCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            return Ok(result.Value);
        }

        [HttpPost]
        [Route("UpdateDiscountCode")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDiscountCode([FromBody] UpdateDiscountCodeCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            return Ok(result.Value);
        }

        [HttpPost]
        [Route("DeleteDiscountCode")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteDiscountCode([FromQuery] int id)
        {
            var result = await _mediator.Send(new DeleteDiscountCodeCommand { Id = id });

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            return Ok(result.Value);
        }
    }
}
