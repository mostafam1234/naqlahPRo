using Application.Features.AdminSection.MainCategoryFeatures.Commands;
using Application.Features.AdminSection.MainCategoryFeatures.Dtos;
using Application.Features.AdminSection.MainCategoryFeatures.Queries;
using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Features.AdminSection.OrderFeature.Queries;
using Application.Features.VehicleSection.Commands;
using Domain.InterFaces;
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
    public class MainCategoryAdminController: ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;
        public MainCategoryAdminController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<MainCategoryAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllMainCategories")]
        public async Task<IActionResult> GetAllMainCategories([FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? searchTerm = null)
        {

            var result = await mediator.Send(new GetAllMainCategoriesQueries
            {
                Skip = skip,
                Take = take,
                SearchTerm = searchTerm
            });

            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<MainCategoryAdminLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetAllMainCategoriesLookup")]
        public async Task<IActionResult> GetAllMainCategoriesLookup()
        {

            var result = await mediator.Send(new GetAllMainCategoriesLookup
            {
                LanguageId = this.userSession.LanguageId
            });

            return Ok(result);

        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AddMainCategoryAdmin")]
        public async Task<IActionResult> AddMainCategoryAdmin(AddMainAdminCategory command)
        {
            var result = await mediator.Send(new AddMainAdminCategory
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
        [Route("DeleteMainCategoryAdmin")]
        public async Task<IActionResult> DeleteMainCategoryAdmin(int mainCategoryId)
        {
            var result = await mediator.Send(new DeleteMainAdminCategory
            {
                Id = mainCategoryId
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
        [Route("UpdateMainCategoryAdmin")]
        public async Task<IActionResult> UpdateMainCategoryAdmin(UpdateMainAdminCategory command)
        {
            var result = await mediator.Send(new UpdateMainAdminCategory
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
