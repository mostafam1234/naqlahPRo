using Application.Features.AdminSection.TechSupportFeatures.Complains.Dtos;
using Application.Features.AdminSection.TechSupportFeatures.Complains.Queries;
using Application.Features.AdminSection.TechSupportFeatures.Suggestions.Dtos;
using Application.Features.AdminSection.TechSupportFeatures.Suggestions.Queries;
using Application.Shared.Dtos;
using Domain.InterFaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Reponse;
using System.Threading.Tasks;

namespace Presentaion.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TechSupportAdminController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;

        public TechSupportAdminController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ComplainDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllComplains")]
        public async Task<IActionResult> GetAllComplains(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await mediator.Send(new GetAllComplainsQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<SuggestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllSuggestions")]
        public async Task<IActionResult> GetAllSuggestions(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await mediator.Send(new GetAllSuggestionsQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }
    }
}

