using Application.Features.CustomerSection.Feature.OrderPack.Dtos;
using Application.Features.CustomerSection.Feature.OrderPack.Queries;
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
    public class OrderPackController : ControllerBase
    {
        private readonly IMediator mediator;

        public OrderPackController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<OrderPackDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await mediator.Send(new GetOrderPackQuery());

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }
    }
}