using Application.Features.AdminSection.CityFeatures.Commands;
using Application.Features.AdminSection.CityFeatures.Dtos;
using Application.Features.AdminSection.CityFeatures.Queries;
using Application.Features.AdminSection.NeighborhoodFeatures.Commands;
using Application.Features.AdminSection.NeighborhoodFeatures.Dtos;
using Application.Features.AdminSection.NeighborhoodFeatures.Queries;
using Application.Features.AdminSection.RegionFeatures.Commands;
using Application.Features.AdminSection.RegionFeatures.Dtos;
using Application.Features.AdminSection.RegionFeatures.Queries;
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
    public class AreasSettingsAdminController: ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;
        public AreasSettingsAdminController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        // Region (Countries) endpoints
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<RegionAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllRegions")]
        public async Task<IActionResult> GetAllRegions([FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await mediator.Send(new GetAllRegionsQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RegionLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllRegionsLookup")]
        public async Task<IActionResult> GetAllRegionsLookup()
        {
            var result = await mediator.Send(new GetAllRegionsLookupQuery
            {
                LanguageId = this.userSession.LanguageId
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
        [Route("AddRegion")]
        public async Task<IActionResult> AddRegion([FromBody] AddRegionCommand command)
        {
            var result = await mediator.Send(new AddRegionCommand
            {
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName
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
        [Route("UpdateRegion")]
        public async Task<IActionResult> UpdateRegion([FromBody] UpdateRegionCommand command)
        {
            var result = await mediator.Send(new UpdateRegionCommand
            {
                Id = command.Id,
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName
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
        [Route("DeleteRegion")]
        public async Task<IActionResult> DeleteRegion([FromQuery] int regionId)
        {
            var result = await mediator.Send(new DeleteRegionCommand
            {
                Id = regionId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        // City endpoints
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CityAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllCities")]
        public async Task<IActionResult> GetAllCities([FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? regionId = null)
        {
            var result = await mediator.Send(new GetAllCitiesQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                RegionId = regionId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CityLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllCitiesLookup")]
        public async Task<IActionResult> GetAllCitiesLookup([FromQuery] int? regionId = null)
        {
            var result = await mediator.Send(new GetAllCitiesLookupQuery
            {
                LanguageId = this.userSession.LanguageId,
                RegionId = regionId
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
        [Route("AddCity")]
        public async Task<IActionResult> AddCity([FromBody] AddCityCommand command)
        {
            var result = await mediator.Send(new AddCityCommand
            {
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName,
                RegionId = command.RegionId
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
        [Route("UpdateCity")]
        public async Task<IActionResult> UpdateCity([FromBody] UpdateCityCommand command)
        {
            var result = await mediator.Send(new UpdateCityCommand
            {
                Id = command.Id,
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName,
                RegionId = command.RegionId
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
        [Route("DeleteCity")]
        public async Task<IActionResult> DeleteCity([FromQuery] int cityId)
        {
            var result = await mediator.Send(new DeleteCityCommand
            {
                Id = cityId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        // Neighborhood endpoints
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<NeighborhoodAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllNeighborhoods")]
        public async Task<IActionResult> GetAllNeighborhoods([FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? cityId = null)
        {
            var result = await mediator.Send(new GetAllNeighborhoodsQuery
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm,
                CityId = cityId
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
        [Route("AddNeighborhood")]
        public async Task<IActionResult> AddNeighborhood([FromBody] AddNeighborhoodCommand command)
        {
            var result = await mediator.Send(new AddNeighborhoodCommand
            {
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName,
                CityId = command.CityId
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
        [Route("UpdateNeighborhood")]
        public async Task<IActionResult> UpdateNeighborhood([FromBody] UpdateNeighborhoodCommand command)
        {
            var result = await mediator.Send(new UpdateNeighborhoodCommand
            {
                Id = command.Id,
                ArabicName = command.ArabicName,
                EnglishName = command.EnglishName,
                CityId = command.CityId
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
        [Route("DeleteNeighborhood")]
        public async Task<IActionResult> DeleteNeighborhood([FromQuery] int neighborhoodId)
        {
            var result = await mediator.Send(new DeleteNeighborhoodCommand
            {
                Id = neighborhoodId
            });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }
    }
}


