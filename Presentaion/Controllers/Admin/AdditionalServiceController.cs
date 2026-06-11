using Application.Features.AdminSection.AdditionalService.Commands;
using Application.Features.AdminSection.AdditionalService.Dtos;
using Application.Features.AdminSection.AdditionalService.Queries;
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
    public class AdditionalServiceController : ControllerBase
    {
        private readonly IMediator mediator;
        public AdditionalServiceController(IMediator mediator) => this.mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AdditionalServiceAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int skip = 0,
                                                [FromQuery] int take = 10,
                                                [FromQuery] string? searchTerm = null)
        {
            var result = await mediator.Send(new GetAllAdditionalServicesQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm
            });
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Add")]
        public async Task<IActionResult> Add([FromBody] AddAdditionalServiceCommand command)
        {
            var result = await mediator.Send(command);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateAdditionalServiceCommand command)
        {
            var result = await mediator.Send(command);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var result = await mediator.Send(new DeleteAdditionalServiceCommand { Id = id });
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }
    }
}
