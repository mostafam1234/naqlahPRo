using Application.Features.AdminSection.AuditFeature.Queries;
using Application.Shared.Dtos;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Authorization;
using Presentaion.Reponse;
using System.Threading.Tasks;

namespace Presentaion.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequirePermission(PermissionNames.CanViewAuditLog)]
    public class AuditAdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditAdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("GetAuditLogs")]
        [ProducesResponseType(typeof(PagedResult<Application.Features.AdminSection.AuditFeature.Dtos.AuditLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] int? userId = null,
            [FromQuery] System.DateTime? fromDate = null,
            [FromQuery] System.DateTime? toDate = null,
            [FromQuery] string? actionName = null,
            [FromQuery] string? entityType = null)
        {
            var result = await _mediator.Send(new GetAuditLogsQuery
            {
                Skip = skip,
                Take = take,
                UserId = userId,
                FromDate = fromDate,
                ToDate = toDate,
                ActionName = actionName,
                EntityType = entityType
            });

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [Route("GetAuditFilterOptions")]
        [ProducesResponseType(typeof(Application.Features.AdminSection.AuditFeature.Dtos.AuditFilterOptionsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAuditFilterOptions()
        {
            var result = await _mediator.Send(new GetAuditFilterOptionsQuery());
            if (result.IsSuccess)
                return Ok(result.Value);
            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }
    }
}
