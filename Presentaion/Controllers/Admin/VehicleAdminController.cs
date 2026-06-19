using Application.Features.CustomerSection.Feature.MainCategory.Dtos;
using Application.Features.CustomerSection.Feature.MainCategory.Queries;
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
        [ProducesResponseType(typeof(List<VehicleBrandDto>), StatusCodes.Status200OK)]
        [Route("GetVehiclesBrandLookup")]
        public async Task<IActionResult> GetVehiclesBrandLookup()
        {
          var result = await mediator.Send(new GetVehicleBrandQuery());
          return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VehicleTypeDto>), StatusCodes.Status200OK)]
        [Route("GetVehiclesTypesLookup")]
        public async Task<IActionResult> GetVehiclesTypesLookup()
        {
          var result = await mediator.Send(new GetVehiceTypesQuery());
          return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ActiveCategoryDto>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(List<VehicleLoadCategoryLookupDto>), StatusCodes.Status200OK)]
        [Route("GetVehicleLoadCategoriesLookup")]
        public async Task<IActionResult> GetVehicleLoadCategoriesLookup()
        {
            var result = await mediator.Send(new GetVehicleLoadCategoriesLookupQuery
            {
                LanguageId = userSession.LanguageId
            });

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(VehicleTypeStatisticsDto), StatusCodes.Status200OK)]
        [Route("GetVehicleTypeStatistics")]
        public async Task<IActionResult> GetVehicleTypeStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await mediator.Send(new GetVehicleTypeStatisticsQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                LanguageId = userSession.LanguageId
            });

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [Route("ExportVehicleTypeStatistics")]
        public async Task<IActionResult> ExportVehicleTypeStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await mediator.Send(new ExportVehicleTypeStatisticsToExcelQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                LanguageId = userSession.LanguageId
            });

            if (result.IsFailure)
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));

            var exportResult = result.Value;
            return File(exportResult.Stream, exportResult.ContentType, exportResult.FileName);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<DeliveryManVehicleDto>), StatusCodes.Status200OK)]
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
        [Route("AddVehicleType")]
        public async Task<IActionResult> AddVehicleType(AddVehicleTypeCommand command)
        {
          var result = await mediator.Send(new AddVehicleTypeCommand
          {
            ArabicName = command.ArabicName,
            EnglishName = command.EnglishName,
            IconBase64 = command.IconBase64,
            MainCategoryIds = command.MainCategoryIds,
            Cost = command.Cost,
            ServiceFee = command.ServiceFee,
            LoadCategory = command.LoadCategory
          });

          if (result.IsSuccess)
          {
            return Ok(result.Value);
          }

          return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [Route("UpdateVehicleType")]
        public async Task<IActionResult> UpdateVehicleType(UpdateVehicleTypeCommand command)
        {
            var result = await mediator.Send(new UpdateVehicleTypeCommand
            {
                VehicleTypeId = command.VehicleTypeId,
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName,
                IconBase64 = command.IconBase64,
                MainCategoryIds = command.MainCategoryIds,
                Cost = command.Cost,
                ServiceFee = command.ServiceFee,
                LoadCategory = command.LoadCategory
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
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
