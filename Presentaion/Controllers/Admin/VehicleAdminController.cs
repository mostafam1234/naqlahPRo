using Application.Features.DeliveryManSection.NewRequests.Dtos;
using Application.Features.DeliveryManSection.NewRequests.Queries;
using Application.Features.DeliveryManSection.Regestration.Dtos;
using Application.Features.DeliveryManSection.Regestration.Qureies;
using Application.Features.VehicleSection.Commands;
using Application.Features.VehicleSection.Dtos;
using Application.Features.VehicleSection.Queries;
using MediatR;
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
  public class VehicleAdminController : ControllerBase
  {
    private readonly IMediator mediator;

    public VehicleAdminController(IMediator mediator)
    {
      this.mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
    [Route("GetVehiclesBrandLookup")]
    public async Task<IActionResult> GetVehiclesBrandLookup()
    {
      var result = await mediator.Send(new GetVehicleBrandQuery());
      return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
    [Route("GetVehiclesTypesLookup")]
    public async Task<IActionResult> GetVehiclesTypesLookup()
    {
      var result = await mediator.Send(new GetVehiceTypesQuery());
      return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VehicleDto>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(PagedResult<VehicleDto>), StatusCodes.Status200OK)]
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
        EnglishName = command.EnglishName
      });

      if (result.IsSuccess)
      {
        return Ok(result.Value);
      }

      return BadRequest(result.Error);
    }

  }
}
