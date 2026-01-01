using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NAQLAH.Server.Services;
using System.Globalization;
using System.Security.Claims;

namespace NAQLAH.Server.MiddleWares
{
    public class SessionInfoMiddleWare
    {
        private readonly RequestDelegate next;

        public SessionInfoMiddleWare(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task Invoke(HttpContext context, [FromServices] UserSession sessionInfo)
        {

            var userClaims = context.User.Claims.ToList();
            var userId = context.User
                                .Claims
                                .FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))
                                ?.Value ?? "";

            var role = context.User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Role))?.Value ?? "";
            var userName = context.User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name))?.Value ?? "";

            var phoneNumber = userClaims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name))?.Value ?? "";


            StringValues lang;
            context.Request.Headers.TryGetValue("Accept-Language", out lang);
           
            // Convert StringValues to string and handle language detection
            var languageValue = lang.ToString().ToLower().Trim();
            sessionInfo.LanguageId = languageValue == "ar" ? (int)Language.Arabic : (int)Language.English;
            sessionInfo.Username = userName;
            sessionInfo.UserId = string.IsNullOrEmpty(userId) ? 0 : int.Parse(userId);
            sessionInfo.PhoneNumber = phoneNumber;
            sessionInfo.UserRole = role;
            await next.Invoke(context);
        }
    }
}
