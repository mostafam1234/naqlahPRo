using Application.Features.CustomerSection.Feature.MainCategory.Dtos;
using Application.Features.CustomerSection.Feature.MainCategory.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Reponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentaion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainCategoryController : ControllerBase
    {
        private readonly IMediator mediator;

        public MainCategoryController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ActiveCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetActiveCategories")]
        public async Task<IActionResult> GetActiveCategories()
        {
            var result = await mediator.Send(new GetActiveCategoriesQuery());

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }
    }
}