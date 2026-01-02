using Application.Features.AdminSection.AssistantWork.Commands;
using Application.Features.AdminSection.AssistantWork.Dtos;
using Application.Features.AdminSection.AssistantWork.Queries;
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
    public class AssistantWorkController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;
        
        public AssistantWorkController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AssistantWorkAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllAssistantWorks")]
        public async Task<IActionResult> GetAllAssistantWorks([FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await mediator.Send(new GetAllAssistantWorksQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AddAssistantWork")]
        public async Task<IActionResult> AddAssistantWork(AddAssistantWorkCommand command)
        {
            var result = await mediator.Send(new AddAssistantWorkCommand
            {
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName,
                Cost = command.Cost
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("DeleteAssistantWork")]
        public async Task<IActionResult> DeleteAssistantWork(int assistantWorkId)
        {
            var result = await mediator.Send(new DeleteAssistantWorkCommand
            {
                Id = assistantWorkId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("UpdateAssistantWork")]
        public async Task<IActionResult> UpdateAssistantWork(UpdateAssistantWorkCommand command)
        {
            var result = await mediator.Send(new UpdateAssistantWorkCommand
            {
                Id = command.Id,
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName,
                Cost = command.Cost
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
    }
}

