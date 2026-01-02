using Application.Features.CustomerSection.Feature.CustomerInfo.Dtos;
using Application.Features.CustomerSection.Feature.CustomerInfo.Queries;
using Application.Features.CustomerSection.Feature.Regestration.Commands;
using Application.Features.CustomerSection.Feature.Regestration.Dtos;
using Application.Features.DeliveryManSection.LogIn.Dtos;
using Domain.InterFaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentaion.Reponse;


namespace Presentaion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly INaqlahContext context;

        public CustomerController(IMediator mediator,
                                  INaqlahContext context)
        {
            this.mediator = mediator;
            this.context = context;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("RegisterAsIndividual")]
        public async Task<IActionResult> RegisterAsIndividual([FromBody] IndividualCustomerRequest registerRequest)
        {

            var result = await mediator.Send(new RegisterCustomerAsIndividualCommand
            {
                PhoneNumber = registerRequest.PhoneNumber,
                BackIdentityImage = registerRequest.BackIdentityImage,
                FrontIdentityImage = registerRequest.FrontIdentityImage,
                IdentityNumber = registerRequest.IdentityNumber,
                Password = registerRequest.Password
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok();
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("RegenerateActivationCode")]
        public async Task<IActionResult> RegenerateActivationCode([FromBody] RegenerateActivationCodeRequest request)
        {

            var result = await mediator.Send(new RegenerateActivationCodeCommand
            {
              PhoneNumber=request.PhoneNumber
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetActivationCode")]
        public async Task<IActionResult> GetActivationCode(string phoneNumber)
        {

            var activeCode = await context.Users
                                        .Where(x => x.PhoneNumber == phoneNumber)
                                        .Select(x => x.ActivationCode)
                                        .FirstOrDefaultAsync();
            return Ok(activeCode);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Activate")]
        public async Task<IActionResult> Activate([FromBody] ActivationRequest request)
        {

            var result = await mediator.Send(new ActivateCustomerCommand
            {
                PhoneNumber = request.PhoneNumber,
                ActiveCode = request.ActiveCode
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok();
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AddDevices")]
        public async Task<IActionResult> AddDevices([FromBody] DeliveryDeviceTokensDto request)
        {

            var result = await mediator.Send(new SaveFireBaseTokensForCustomerCommand
            {
                AndroidDevice = request.AndriodDevice,
                IosDevice = request.IosDevice
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok();
        }


        [HttpPost]
        [ProducesResponseType(typeof(CustomerLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] LoginRquestDto request)
        {

            var result = await mediator.Send(new CustomerLoginCommand
            {
                PhoneNmber=request.UserName,
                Password=request.Password
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }





        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("RegisterAsEstablishMent")]
        public async Task<IActionResult> RegisterAsEstablishMent([FromBody] EstablishMentCustomerRequest registerRequest)
        {

            var result = await mediator.Send(new RegisterCustomerAsEstablishMentCommand
            {
                MobileNumber=registerRequest.MobileNumber,
                Name=registerRequest.Name,
                Address=registerRequest.Address,
                RecoredImage=registerRequest.RecoredImage,
                RepresentitveBackIdentityNumberImage=registerRequest.RepresentitveBackIdentityNumberImage,
                RepresentitveFrontIdentityNumberImage=registerRequest.RepresentitveFrontIdentityNumberImage,
                RepresentitveName=registerRequest.RepresentitveName,
                RepresentitvePhoneNumber=registerRequest.RepresentitvePhoneNumber,
                TaxRegistrationImage=registerRequest.TaxRegistrationImage,
                TaxRegistrationNumber=registerRequest.TaxRegistrationNumber,
                Password = registerRequest.Password
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(CustomerInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetCustomerInfo")]
        public async Task<IActionResult> GetCustomerInfo()
        {
            var result = await mediator.Send(new GetCustomerInfoQuery
            {
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }
    }
}
