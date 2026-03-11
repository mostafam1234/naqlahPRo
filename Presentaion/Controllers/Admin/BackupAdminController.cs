using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Domain.Constants;
using Domain.InterFaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Authorization;
using Presentaion.Reponse;
using System;
using System.Threading.Tasks;

namespace Presentaion.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequirePermission(PermissionNames.CanExportData)]
    public class BackupAdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;

        public BackupAdminController(IMediator mediator, IUserSession userSession)
        {
            _mediator = mediator;
            _userSession = userSession;
        }

        /// <summary>
        /// Export a single module to Excel. Optional from/to date filter (applies only to date-filterable modules).
        /// </summary>
        [HttpGet("Export")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Export(
            [FromQuery] string module,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var query = new ExportModuleToExcelQuery
            {
                ModuleKey = module ?? string.Empty,
                FromDate = from,
                ToDate = to,
                LanguageId = _userSession.LanguageId
            };

            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            var exportResult = result.Value;
            return File(exportResult.Stream, exportResult.ContentType, exportResult.FileName);
        }
    }
}
