using Application.Features.AdminSection.SystemUsers.Commands;
using Application.Features.AdminSection.SystemUsers.Dtos;
using Application.Features.AdminSection.SystemUsers.Queries;
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
    public class SystemUserAdminController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;

        public SystemUserAdminController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<SystemUserAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllSystemUsers")]
        public async Task<IActionResult> GetAllSystemUsers(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? roleFilter = null)
        {
            var result = await mediator.Send(new GetAllSystemUsersQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                RoleFilter = roleFilter
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(SystemUserAdminDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetSystemUserById")]
        public async Task<IActionResult> GetSystemUserById([FromQuery] int id)
        {
            var result = await mediator.Send(new GetSystemUserByIdQuery { Id = id });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AddSystemUser")]
        public async Task<IActionResult> AddSystemUser([FromBody] AddSystemUserCommand command)
        {
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("UpdateSystemUser")]
        public async Task<IActionResult> UpdateSystemUser([FromBody] UpdateSystemUserCommand command)
        {
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("DeleteSystemUser")]
        public async Task<IActionResult> DeleteSystemUser([FromQuery] int id)
        {
            var result = await mediator.Send(new DeleteSystemUserCommand { Id = id });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RoleLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllRolesLookup")]
        public async Task<IActionResult> GetAllRolesLookup()
        {
            var result = await mediator.Send(new GetAllRolesLookupQuery());

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }
    }
}

