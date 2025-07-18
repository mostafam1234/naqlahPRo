using Application.Features.DeliveryManSection.LogIn;
using Application.Features.DeliveryManSection.LogIn.Commands;
using Application.Features.DeliveryManSection.LogIn.Dtos;
using Application.Features.DeliveryManSection.LogIn.Queries;
using Application.Features.DeliveryManSection.Regestration.Commands;
using Application.Features.DeliveryManSection.Regestration.Dtos;
using Application.Features.DeliveryManSection.Regestration.Qureies;
using Domain.Shared;
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

namespace Presentaion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryManController : ControllerBase
    {
        private readonly IMediator mediator;

        public DeliveryManController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(DeliveryManTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] DeliveryRegisterRequest registerRequest)
        {

            var result = await mediator.Send(new RegisterNewDeliveryManCommand
            {
                Email = registerRequest.Email,
                Name = registerRequest.Name,
                Password = registerRequest.Password,
                PhoneNumber = registerRequest.PhoneNumber
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }


        [HttpPost]
        [ProducesResponseType(typeof(DeliveryManTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] LoginRquestDto registerRequest)
        {

            var result = await mediator.Send(new DeliveryManLogInCommand
            {
                UserName = registerRequest.UserName,
                Password = registerRequest.Password
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(DeliveryManInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Info")]
        public async Task<IActionResult> Info()
        {

            var result = await mediator.Send(new GetDeliveryManInfoQuery());
           

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("CreatePersonalInfo")]
        [Authorize]
        public async Task<IActionResult> CreatePersonalInfo([FromBody] DeliveryPersonalInfoRequest request)
        {

            var result = await mediator.Send(new SaveDeliveryManInfoCommand
            {
                FullName = request.FullName,
                Address = request.FullName,
                IdentityNumber = request.IdentityNumber,
                FrontIdenitytImage = request.FrontIdenitytImage,
                BackIdenitytImage = request.BackIdenitytImage,
                FrontDrivingLicenseImage = request.FrontDrivingLicenseImage,
                BackDrivingLicenseImage = request.BackDrivingLicenseImage,
                DeliveryTypeId = request.DeliveryTypeId,
                DeliveryLicenseTypeId = request.DeliveryLicenseTypeId,
                PersonalImage = request.PersonalImage,
                DrivingLicenseExpirationDate = request.DrivingLicenseExpirationDate,
                IdentityExpirationDate = request.DrivingLicenseExpirationDate
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
        [Route("AddCarOwnerResidentInfo")]
        [Authorize]
        public async Task<IActionResult> AddCarOwnerResidentInfo([FromBody] ResidentRequest request)
        {
            var result = await mediator.Send(new SaveDeliveryCarOwnerAsResidentCommand
            {
                BankAccountNumber = request.BankAccountNumber,
                CitizenName = request.CitizenName,
                IdentityNumber = request.IdentityNumber,
                BackIdentityImage = request.BackIdentityImage,
                FrontIdentityImage = request.FrontIdentityImage
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
        [Route("AddCarOwnerCompanyInfo")]
        [Authorize]
        public async Task<IActionResult> AddCarOwnerCompanyInfo([FromBody] CompanyRequest request)
        {
            var result = await mediator.Send(new SaveDeliveryCarOwnerAsCompanyCommand
            {
                CompanyName = request.CompanyName,
                CommercialRecordNumber = request.CommercialRecordNumber,
                BankAccountNumber = request.BankAccountNumber,
                RecordImagePath = request.RecordImagePath,
                TaxCertificateImage = request.TaxCertificateImage,
                TaxNumber = request.TaxNumber
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
        [Route("AddCarOwnerRenterInfo")]
        [Authorize]
        public async Task<IActionResult> AddCarOwnerRenterInfo([FromBody] RenterRequest request)
        {

            var result=await mediator.Send(new SaveDeliveryCarOwnerAsRenterCommand
            {
                CitizenName=request.CitizenName,
                RentContractImage=request.RentContractImage,
                BankAccountNumber=request.BankAccountNumber,
                BackIdentityImage=request.BackIdentityImage,
                FrontIdentityImage=request.FrontIdentityImage,
                IdentityNumber=request.IdentityNumber
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
        [Route("AddVehicle")]
        [Authorize]
        public async Task<IActionResult> AddVehicle([FromBody] AddDeliveryVehicleRequest request)
        {

            var result = await mediator.Send(new SaveDeliveryVehicleInfoCommand
            {
                BackInsuranceImagePath = request.BackInsuranceImagePath,
                BackLicenseImagePath = request.BackLicenseImagePath,
                FrontImagePath = request.FrontImagePath,
                FrontInsuranceImagePath = request.FrontInsuranceImagePath,
                FrontLicenseImagePath = request.FrontLicenseImagePath,
                InSuranceExpirationDate = request.InSuranceExpirationDate,
                LicenseExpirationDate = request.LicenseExpirationDate,
                LicensePlateNumber = request.LicensePlateNumber,
                SideImagePath = request.SideImagePath,
                VehicleBrandId = request.VehicleBrandId,
                VehicleOwnerTypeId = request.VehicleOwnerTypeId,
                VehicleTypeId = request.VehicleTypeId,
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VehicleBrandDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("VehicleBrands")]
        public async Task<IActionResult> VehicleBrands()
        {
            var result = await mediator.Send(new GetVehicleBrandQuery());
            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VehicleTypeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("VehicleTypes")]
        public async Task<IActionResult> VehicleTypes()
        {
            var result = await mediator.Send(new GetVehiceTypesQuery());
            return Ok(result.Value);
        }
    }
}
