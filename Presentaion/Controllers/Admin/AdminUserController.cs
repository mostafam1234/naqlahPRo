using Application.Features.AdminSection.LogIn;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
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
    public class AdminUserController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IUserSession userSession;
        
        public AdminUserController(IMediator mediator, IUserSession userSession)
        {
            this.mediator = mediator;
            this.userSession = userSession;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AdminResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("LoginAdmin")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginAdminCommand command)
        {
            var result = await mediator.Send(command);
            
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Logout")]
        [Authorize] // محمي بالتوثيق
        public async Task<IActionResult> Logout()
        {
            try
            {
                // لوج عملية تسجيل الخروج
                Console.WriteLine($"Admin user {userSession.Username} (ID: {userSession.UserId}) logged out at {DateTime.Now}");

                // إرجاع استجابة نجاح
                return Ok(new { 
                    Message = "تم تسجيل الخروج بنجاح",
                    Success = true,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                // في حالة حدوث خطأ، ما زال بإمكان المستخدم تسجيل الخروج من الفرونت
                return Ok(new { 
                    Message = "تم تسجيل الخروج",
                    Success = true,
                    Error = ex.Message
                });
            }
        }
    }
}
