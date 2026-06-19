using Application.Features.AdminSection.CustomerAdminFeature.Commands;
using Application.Features.AdminSection.CustomerAdminFeature.Dtos;
using Application.Features.AdminSection.CustomerAdminFeature.Queries;
using Application.Shared.Dtos;
using Domain.Constants;
using Domain.InterFaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Authorization;
using Presentaion.Reponse;

namespace Presentaion.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerAdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;

        public CustomerAdminController(IMediator mediator, IUserSession userSession)
        {
            _mediator = mediator;
            _userSession = userSession;
        }

        [HttpGet]
        [RequirePermission(PermissionNames.CanViewCustomers)]
        [ProducesResponseType(typeof(PagedResult<AdminCustomerListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllCustomers")]
        public async Task<IActionResult> GetAllCustomers(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _mediator.Send(new GetAllCustomersAdminQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                LanguageId = _userSession.LanguageId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [RequirePermission(PermissionNames.CanViewCustomers)]
        [ProducesResponseType(typeof(AdminCustomerDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetCustomerDetail")]
        public async Task<IActionResult> GetCustomerDetail([FromQuery] int customerId)
        {
            var result = await _mediator.Send(new GetCustomerAdminByIdQuery
            {
                CustomerId = customerId,
                LanguageId = _userSession.LanguageId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [RequirePermission(PermissionNames.CustomerAdminActions)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("SetCustomerUserActive")]
        public async Task<IActionResult> SetCustomerUserActive([FromQuery] int customerId, [FromQuery] bool isActive)
        {
            var result = await _mediator.Send(new SetCustomerUserActiveCommand
            {
                CustomerId = customerId,
                IsActive = isActive
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [RequirePermission(PermissionNames.CustomerAdminActions)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("DeleteCustomerUser")]
        public async Task<IActionResult> DeleteCustomerUser([FromQuery] int customerId)
        {
            var result = await _mediator.Send(new DeleteCustomerUserCommand { CustomerId = customerId });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [RequirePermission(PermissionNames.CustomerAdminActions)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("RestoreCustomerUser")]
        public async Task<IActionResult> RestoreCustomerUser([FromQuery] int customerId)
        {
            var result = await _mediator.Send(new RestoreCustomerUserCommand { CustomerId = customerId });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [RequirePermission(PermissionNames.CustomerAdminActions)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("ResetCustomerPassword")]
        public async Task<IActionResult> ResetCustomerPassword(
            [FromQuery] int customerId,
            [FromBody] AdminResetCustomerPasswordRequest body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.NewPassword))
            {
                return BadRequest(ProblemDetail.CreateProblemDetail("كلمة المرور الجديدة مطلوبة"));
            }

            var result = await _mediator.Send(new AdminResetCustomerPasswordCommand
            {
                CustomerId = customerId,
                NewPassword = body.NewPassword
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }
    }
}
