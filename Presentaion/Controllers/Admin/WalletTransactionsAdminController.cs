using Application.Features.AdminSection.WalletTransactionFeatures.Commands;
using Application.Features.AdminSection.WalletTransactionFeatures.Dtos;
using Application.Features.AdminSection.WalletTransactionFeatures.Queries;
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
    public class WalletTransactionsAdminController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;
        public WalletTransactionsAdminController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<WalletTransactionAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllWalletTransactions")]
        public async Task<IActionResult> GetAllWalletTransactions(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? withdraw = null)
        {
            var result = await mediator.Send(new GetAllWalletTransactionsQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                CustomerId = customerId,
                FromDate = fromDate,
                ToDate = toDate,
                Withdraw = withdraw
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CustomerLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetCustomersLookup")]
        public async Task<IActionResult> GetCustomersLookup([FromQuery] string? searchTerm = null)
        {
            var result = await mediator.Send(new GetCustomersLookupQuery
            {
                SearchTerm = searchTerm
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AddWalletTransaction")]
        public async Task<IActionResult> AddWalletTransaction([FromBody] AddWalletTransactionCommand command)
        {
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetWalletBalance")]
        public async Task<IActionResult> GetWalletBalance(
            [FromQuery] int customerId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? withdraw = null)
        {
            var result = await mediator.Send(new GetWalletBalanceQuery
            {
                CustomerId = customerId,
                FromDate = fromDate,
                ToDate = toDate,
                Withdraw = withdraw
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }
    }
}


