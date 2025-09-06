using Application.Features.CustomerSection.Feature.Order.Commands;
using Application.Features.CustomerSection.Feature.Order.Dtos;
using Application.Features.CustomerSection.Feature.Regestration.Commands;
using Application.Features.CustomerSection.Feature.Regestration.Dtos;
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
    public class CustomerOrderController:ControllerBase
    {
        private readonly IMediator mediator;
        public CustomerOrderController(IMediator mediator)
        {
            this.mediator = mediator;
        }


        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderResponseDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto request)
        {
            var result = await mediator.Send(new CreateNewOrderCommand
            {
                Order=request,
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SelectVehicleTypeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("SelectVehicleType")]
        public async Task<IActionResult> SelectVehicleType([FromBody] SelectVehicleTypeDto request)
        {
            var result = await mediator.Send(new SelectVehicleTypeForOrderCommand(request));

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

    }
}
