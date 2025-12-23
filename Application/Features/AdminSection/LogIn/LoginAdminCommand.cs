using Application.Features.CustomerSection.Feature.Regestration.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.LogIn
{
    public class LoginAdminCommand : IRequest<Result<AdminResponse>>
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        private class LoginAdminCommandHandler : IRequestHandler<LoginAdminCommand, Result<AdminResponse>>
        {
            private readonly IUserService userService;
            private readonly INaqlahContext context;
            
            public LoginAdminCommandHandler(INaqlahContext context, IUserService userService)
            {
                this.userService = userService;
                this.context = context;
            }
            
            public async Task<Result<AdminResponse>> Handle(LoginAdminCommand request, CancellationToken cancellationToken)
            {
                // البحث بالـ Email (UserName هنا هو Email فعلياً)
                var user = await context.Users.FirstOrDefaultAsync(x => x.Email == request.UserName, cancellationToken);
                if (user is null)
                {
                    return Result.Failure<AdminResponse>("البريد الإلكترونى غير مسجل من قبل !");
                }

                // استخدام الدالة الجديدة للأدمن
                var accessToken = await userService.GetAdminAccessToken(request.UserName, request.Password);
                if (accessToken.IsFailure)
                {
                    return Result.Failure<AdminResponse>(accessToken.Error);
                }

                return Result.Success(new AdminResponse
                {
                    IsActive = true,
                    TokenResponse = accessToken.Value
                });
            }
        }
    }
}
