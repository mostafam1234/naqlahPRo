using Application.Features.AdminSection.NotificationFeature.Commands;
using Application.Features.AdminSection.NotificationFeature.Dtos;
using Application.Features.AdminSection.NotificationFeature.Queries;
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
    public class NotificationAdminController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;

        public NotificationAdminController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetNotifications")]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] bool? unreadOnly = null)
        {
            var query = new GetNotificationsQuery
            {
                Skip = skip,
                Take = take,
                UnreadOnly = unreadOnly,
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
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetUnreadNotificationsCount")]
        public async Task<IActionResult> GetUnreadNotificationsCount()
        {
            var query = new GetUnreadNotificationsCountQuery();
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
        [Route("MarkNotificationAsRead/{id}")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var command = new MarkNotificationAsReadCommand
            {
                NotificationId = id,
                LanguageId = this.userSession.LanguageId
            };

            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("MarkAllNotificationsAsRead")]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var command = new MarkAllNotificationsAsReadCommand
            {
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

