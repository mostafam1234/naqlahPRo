using Application.Features.AdminSection.BackupFeature.Commands;
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
        private readonly IBackupOperationService _backupOperationService;

        public BackupAdminController(
            IMediator mediator,
            IUserSession userSession,
            IBackupOperationService backupOperationService)
        {
            _mediator = mediator;
            _userSession = userSession;
            _backupOperationService = backupOperationService;
        }

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

        [HttpGet("ExportFullDatabase")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportFullDatabase()
        {
            var result = await _mediator.Send(new ExportFullDatabaseBackupQuery());

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            var exportResult = result.Value;
            return File(exportResult.Stream, exportResult.ContentType, exportResult.FileName);
        }

        [HttpPost("StartFullDatabaseBackup")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult StartFullDatabaseBackup()
        {
            var jobId = _backupOperationService.StartBackup();
            return Ok(new { jobId });
        }

        [HttpPost("StartFullDatabaseRestore")]
        [RequestSizeLimit(524_288_000)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public IActionResult StartFullDatabaseRestore(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ProblemDetail.CreateProblemDetail("BackupFileRequired"));

            var stream = file.OpenReadStream();
            var jobId = _backupOperationService.StartRestore(stream);
            return Ok(new { jobId });
        }

        [HttpGet("OperationStatus/{jobId}")]
        [ProducesResponseType(typeof(DatabaseOperationStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetOperationStatus(string jobId)
        {
            var status = _backupOperationService.GetStatus(jobId);
            if (status == null)
                return NotFound();

            return Ok(status);
        }

        [HttpGet("OperationDownload/{jobId}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DownloadOperationResult(string jobId)
        {
            var status = _backupOperationService.GetStatus(jobId);
            if (status == null || status.Phase != "completed")
                return NotFound();

            var bytes = _backupOperationService.GetBackupFile(jobId);
            if (bytes == null || bytes.Length == 0)
                return NotFound();

            var fileName = status.DownloadFileName ?? "Naqlah_FullBackup.sql";
            return File(bytes, "application/sql", fileName);
        }

        [HttpPost("RestoreFullDatabase")]
        [RequestSizeLimit(524_288_000)]
        [ProducesResponseType(typeof(DatabaseRestoreSummary), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreFullDatabase(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ProblemDetail.CreateProblemDetail("BackupFileRequired"));

            await using var stream = file.OpenReadStream();
            var command = new RestoreFullDatabaseBackupCommand { SqlStream = stream };
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            return Ok(result.Value);
        }
    }
}
