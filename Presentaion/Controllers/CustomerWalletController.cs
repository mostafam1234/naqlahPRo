using Application.Features.CustomerSection.Feature.Wallet.Dtos;
using Application.Features.CustomerSection.Feature.Wallet.Queries;
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
    public class CustomerWalletController : ControllerBase
    {
        private readonly IMediator mediator;

        public CustomerWalletController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CustomerWalletBalanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetBalance")]
        public async Task<IActionResult> GetBalance()
        {
            var result = await mediator.Send(new GetCustomerWalletBalanceQuery());

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }
    }
}