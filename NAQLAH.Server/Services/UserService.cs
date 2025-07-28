using Azure.Core;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using Domain.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace NAQLAH.Server.Services
{
    public class UserService : IUserService
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

        public async Task<Result>CheckUserPassword(string userName,
                                                   string password)
        {
            var user = await userManager.Users
                                        .FirstOrDefaultAsync(x => x.UserName == userName);

            if (user is null)
            {
                var error = readFromResourceFile.GetLocalizedMessage("InvalidUserNameOrPassword");
                return Result.Failure<string>(error);
            }

            var passwordResult = await userManager.CheckPasswordAsync(user,password);
            if (!passwordResult)
            {
                var error = readFromResourceFile.GetLocalizedMessage("InvalidUserNameOrPassword");
                return Result.Failure<string>(error);
            }

            return Result.Success();
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
                var error = readFromResourceFile.GetLocalizedMessage("PhoneNumberAlreadyExist");
                return Result.Failure<int>(error);
            }

            var isEmailAlreadyExsist = await context.Users
                                                    .AnyAsync(x => x.Email == email);

            if (isEmailAlreadyExsist)
            {
                var error = readFromResourceFile.GetLocalizedMessage("EmailAlreadyExist");
                return Result.Failure<int>(error);
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


        public async Task<Result<int>> CreateCustomerUserAsIndividual(string phoneNumber,
                                                                      string identtyNumber,
                                                                      string frontIdentitImage,
                                                                      string backIdentityImag,
                                                                      string password)
        {
            var isPhoneNumberAlreadyExsist = await context.Users
                                                 .AnyAsync(x => x.PhoneNumber == phoneNumber);

            if (isPhoneNumberAlreadyExsist)
            {
                var error = readFromResourceFile.GetLocalizedMessage("PhoneNumberAlreadyExist");
                return Result.Failure<int>(error);
            }


            var customer = User.CreateIndividualCustomerUser(phoneNumber,
                                                             identtyNumber,
                                                             frontIdentitImage,
                                                             backIdentityImag);

            if (customer.IsFailure)
            {
                return Result.Failure<int>(customer.Error);
            }

            var user = customer.Value;
            var registrationResult = await userManager.CreateAsync(user, password);
            if (!registrationResult.Succeeded)
            {
                var errors = registrationResult.Errors.ToList();
                var errorMessage = string.Join(",", errors);
                return Result.Failure<int>(errorMessage);
            }


            return user.Id;
        }



        public async Task<Result<int>> CreateCustomerUserAsEstablishMent(string phoneNumber,
                                                                         string name,
                                                                         string recordImagePath,
                                                                         string taxRegistrationNumber,
                                                                         string taxRegestationImagePath,
                                                                         string address,
                                                                     string establishmentRepresentitveName,
                                                               string establishmentRepresentitveMobile,
                                                               string establishmentRepresentitvefrontImage,
                                                               string establishmentRepresentitveBackImage,
                                                                         string password)
        {
            var isPhoneNumberAlreadyExsist = await context.Users
                                                 .AnyAsync(x => x.PhoneNumber == phoneNumber);

            if (isPhoneNumberAlreadyExsist)
            {
                var error = readFromResourceFile.GetLocalizedMessage("PhoneNumberAlreadyExist");
                return Result.Failure<int>(error);
            }


            var customer = User.CreateEtablishMentCustomerUser(phoneNumber,
                                                               name,
                                                               recordImagePath,
                                                               taxRegistrationNumber,
                                                               taxRegestationImagePath,
                                                               address,
                                                               establishmentRepresentitveName,
                                                               establishmentRepresentitveMobile,
                                                               establishmentRepresentitvefrontImage,
                                                               establishmentRepresentitveBackImage);

            if (customer.IsFailure)
            {
                return Result.Failure<int>(customer.Error);
            }

            var user = customer.Value;
            var registrationResult = await userManager.CreateAsync(user, password);
            if (!registrationResult.Succeeded)
            {
                var errors = registrationResult.Errors.ToList();
                var errorMessage = string.Join(",", errors);
                return Result.Failure<int>(errorMessage);
            }


            return user.Id;
        }


        public async Task<Result<TokenResponse>> GetAcessToken(string userName,
                                                              string Password)
        {
            var client = httpClientFactory.CreateClient();
            var logInRequest = new LoginRequest { Email = userName, Password = Password };
            var apiBaseUrl = readFromAppSetting.GetValue<string>("apiBaseUrl");
            var loginUrl = string.Format("{0}/login", apiBaseUrl);
            var response = await client.PostAsJsonAsync(loginUrl, logInRequest);
            if (!response.IsSuccessStatusCode)
            {
                var error = readFromResourceFile.GetLocalizedMessage("InValidUsernameOrPassword");
                return Result.Failure<TokenResponse>(error);
            }
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return Result.Success<TokenResponse>(result);
        }
    }
}
