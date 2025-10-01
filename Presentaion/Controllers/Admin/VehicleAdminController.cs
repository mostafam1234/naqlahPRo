using Application.Features.CustomerSection.Feature.MainCategory.Dtos;
using Application.Features.CustomerSection.Feature.MainCategory.Queries;
using Application.Features.DeliveryManSection.NewRequests.Dtos;
using Application.Features.DeliveryManSection.NewRequests.Queries;
using Application.Features.DeliveryManSection.Regestration.Dtos;
using Application.Features.DeliveryManSection.Regestration.Qureies;
using Application.Features.VehicleSection.Commands;
using Application.Features.VehicleSection.Dtos;
using Application.Features.VehicleSection.Queries;
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
  public class VehicleAdminController : ControllerBase
  {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;

        public VehicleAdminController(IMediator mediator, IUserSession userSession)
        {
          this.mediator = mediator;
          this.userSession = userSession;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VehicleTypeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetVehiclesBrandLookup")]
        public async Task<IActionResult> GetVehiclesBrandLookup()
        {
          var result = await mediator.Send(new GetVehicleBrandQuery());
          return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VehicleTypeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetVehiclesTypesLookup")]
        public async Task<IActionResult> GetVehiclesTypesLookup()
        {
          var result = await mediator.Send(new GetVehiceTypesQuery());
          return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ActiveCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetMainCategoriesLookup")]
        public async Task<IActionResult> GetMainCategoriesLookup()
        {
          var result = await mediator.Send(new GetActiveCategoriesQuery());
          if (result.IsFailure)
          {
            return BadRequest(result.Error);
          }
          return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<DeliveryManVehicleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetVehiclesTypes")]
        public async Task<IActionResult> GetVehiclesTypes(int skip = 0, int take = 10, string searchterm = "")
        {
          var result = await mediator.Send(new GetVehiclesTypesQueryForDisplaying
          {
            Skip = skip,
            Take = take,
            SearchTerm = searchterm ?? string.Empty
          });
          return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<DeliveryManVehicleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetVehiclesBrands")]
        public async Task<IActionResult> GetVehiclesBrands(int skip = 0, int take = 10, string searchterm = "")
        {
          var result = await mediator.Send(new GetVehiclesBrandsForDisplaying
          {
            Skip = skip,
            Take = take,
            SearchTerm = searchterm ?? string.Empty
          });
          return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AddVehicleBrand")]
        public async Task<IActionResult> AddVehicleBrand(AddVehicleBrandCommand command)
        {
          var result = await mediator.Send(new AddVehicleBrandCommand
          {
            ArabicName = command.ArabicName,
            EnglishName = command.EnglishName
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
        [Route("AddVehicleType")]
        public async Task<IActionResult> AddVehicleType(AddVehicleTypeCommand command)
        {
          var result = await mediator.Send(new AddVehicleTypeCommand
          {
            ArabicName = command.ArabicName,
            EnglishName = command.EnglishName,
            IconBase64 = command.IconBase64,
            MainCategoryIds = command.MainCategoryIds
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
        [Route("UpdateVehicleType")]
        public async Task<IActionResult> UpdateVehicleType(UpdateVehicleTypeCommand command)
        {
            var result = await mediator.Send(new UpdateVehicleTypeCommand
            {
                VehicleTypeId = command.VehicleTypeId,
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName,
                IconBase64 = command.IconBase64,
                MainCategoryIds = command.MainCategoryIds
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
        [Route("UpdateVehicleBrand")]
        public async Task<IActionResult> UpdateVehicleBrand(UpdateVehicleBrandCommand command)
        {
            var result = await mediator.Send(new UpdateVehicleBrandCommand
            {
                VehicleBrandId = command.VehicleBrandId,
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName
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
        [Route("DeleteVehicleType")]
        public async Task<IActionResult> DeleteVehicleType(int vehicleTypeId)
        {
            var result = await mediator.Send(new DeleteVehicleTypeCommand
            {
                VehicleTypeId = vehicleTypeId
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
        [Route("DeleteVehicleBrand")]
        public async Task<IActionResult> DeleteVehicleBrand(int vehicleBrandId)
        {
            var result = await mediator.Send(new DeleteVehicleBrandCommand
            {
                VehicleBrandId = vehicleBrandId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
    }
}
