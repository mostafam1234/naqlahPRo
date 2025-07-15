using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace NAQLAH.Server.Services
{
    public class UserService: IUserService
    {
        private readonly UserManager<User> userManager;
        private readonly IReadFromResourceFile readFromResourceFile;
        private readonly IUserSession userSession;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IReadFromAppSetting readFromAppSetting;
        private readonly INaqlahContext context;

        public UserService(UserManager<User> userManager,
                            IReadFromResourceFile readFromResourceFile,
                            IUserSession userSession,
                            IHttpClientFactory httpClientFactory,
                            IReadFromAppSetting readFromAppSetting,
                            INaqlahContext Context)
        {
            this.userManager = userManager;
            this.readFromResourceFile = readFromResourceFile;
            this.userSession = userSession;
            this.httpClientFactory = httpClientFactory;
            this.readFromAppSetting = readFromAppSetting;
            context = Context;
        }

        public async Task<Result<int>> CreateDeliveryUser(string mobile,
                                                          string email,
                                                          string name,
                                                          string password)
        {
            var isPhoneNumberAlreadyExsist = await context.Users
                                                 .AnyAsync(x => x.PhoneNumber == mobile);

            if (isPhoneNumberAlreadyExsist)
            {
                return Result.Failure<int>("Phone Number is Already Exsist");
            }

            var isEmailAlreadyExsist = await context.Users
                                                    .AnyAsync(x => x.PhoneNumber == mobile);

            if (isEmailAlreadyExsist)
            {
                return Result.Failure<int>("Email is ALready Exsist");
            }


            var userResult = User.CreateDeliveryUser(mobile, email, name);
            if (userResult.IsFailure)
            {
                return Result.Failure<int>(userResult.Error);
            }

            var user = userResult.Value;
            var registrationResult = await userManager.CreateAsync(user, password);
            if (!registrationResult.Succeeded)
            {
                var errors = registrationResult.Errors.ToList();
                var errorMessage = string.Join(",", errors);
                return Result.Failure<int>(errorMessage);
            }


            return user.Id;
        }
    }
}
