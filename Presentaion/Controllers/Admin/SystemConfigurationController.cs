using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Features.AdminSection.OrderFeature.Queries;
using Application.Features.AdminSection.SystemConfiguration.Command;
using Application.Features.AdminSection.SystemConfiguration.Dto;
using Application.Features.AdminSection.SystemConfiguration.Query;
using Application.Shared.Dtos;
using Domain.Enums;
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
    public class SystemConfigurationController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;

        public SystemConfigurationController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SystemConfigurationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetSystemConfiguration")]
        public async Task<IActionResult> GetSystemConfiguration()
        {
            var query = new GetSystemConfigurationQuery();


            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Update")]
        public async Task<IActionResult> Update(UpdateSystemConfigurationCommand request)
        {

            var result = await mediator.Send(request);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(result.Error);
        }

    }
}
