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
        private readonly SignInManager<User> signInManager;

        public UserService(UserManager<User> userManager,
                            IReadFromResourceFile readFromResourceFile,
                            IUserSession userSession,
                            IHttpClientFactory httpClientFactory,
                            IReadFromAppSetting readFromAppSetting,
                            INaqlahContext Context,
                            SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.readFromResourceFile = readFromResourceFile;
            this.userSession = userSession;
            this.httpClientFactory = httpClientFactory;
            this.readFromAppSetting = readFromAppSetting;
            context = Context;
            this.signInManager = signInManager;
        }

        public async Task<Result> CheckUserPassword(string userName,
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

        public async Task<Result> CheckAdminUserPassword(string email,
                                           string password)
        {
            var user = await userManager.Users
                                        .FirstOrDefaultAsync(x => x.Email == email);

            if (user is null)
            {
                var error = "البريد الالكترونى غير مسجل من قبل";
                return Result.Failure<string>(error);
            }

            // التحقق من أن المستخدم أدمن
            var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                var error = "غير مصرح لك بالدخول كأدمن";
                return Result.Failure<string>(error);
            }

            var passwordResult = await userManager.CheckPasswordAsync(user, password);
            if (!passwordResult)
            {
                var error = "البريد الالكترونى او كلمة المرور غير صحيحة";
                return Result.Failure<string>(error);
            }

            return Result.Success();
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

        // دالة جديدة خاصة للأدمن
        public async Task<Result<TokenAdminResponse>> GetAdminAccessToken(string email, string password)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Result.Failure<TokenAdminResponse>("البريد الإلكتروني غير صحيح");
                }

                // التحقق من الدور
                var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin)
                {
                    return Result.Failure<TokenAdminResponse>("غير مصرح لك بالدخول كأدمن");
                }

                // التحقق من كلمة المرور
                var passwordCheck = await userManager.CheckPasswordAsync(user, password);
                if (!passwordCheck)
                {
                    return Result.Failure<TokenAdminResponse>("كلمة المرور غير صحيحة");
                }

                // تسجيل الدخول باستخدام SignInManager وإنشاء Token
                var result = await signInManager.CreateUserPrincipalAsync(user);
                
                // استخدام Identity API لإنشاء Token للمستخدم
                var client = httpClientFactory.CreateClient();
                var logInRequest = new LoginRequest { Email = email, Password = password };
                var apiBaseUrl = readFromAppSetting.GetValue<string>("apiBaseUrl");
                var loginUrl = string.Format("{0}/login", apiBaseUrl);
                
                var response = await client.PostAsJsonAsync(loginUrl, logInRequest);
                if (!response.IsSuccessStatusCode)
                {
                    // إذا فشل، جرب مرة تانية بـ UserName
                    logInRequest = new LoginRequest { Email = user.UserName!, Password = password };
                    response = await client.PostAsJsonAsync(loginUrl, logInRequest);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Result.Failure<TokenAdminResponse>($"فشل في إنشاء التوكن: {errorContent}");
                    }
                }
                
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                var roles = await userManager.GetRolesAsync(user);

                var adminTokenResponse = new TokenAdminResponse
                {
                    AccessToken = tokenResponse!.AccessToken,
                    ExpiresIn = tokenResponse.ExpiresIn,
                    RefreshToken = tokenResponse.RefreshToken,
                    Role = roles.FirstOrDefault()??"Not Found"
                };
                return Result.Success(adminTokenResponse!);
            }
            catch (Exception ex)
            {
                return Result.Failure<TokenAdminResponse>($"خطأ في تسجيل دخول الأدمن: {ex.Message}");
            }
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
    }
}
