using Application.DependencyInjection;
using Domain.Models;
using NAQLAH.Server.ApiDependencyInjection;
using Hangfire;
using Infrastructure;
using Infrastructure.HangFireDepencies;
using Infrastructure.Jobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NAQLAH.Server.MiddleWares;
using Presentaion;
using System.Globalization;

namespace NAQLAH.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await RunAsync(args);
            }
            catch (Exception ex)
            {
                LogStartupFailure(ex);
                throw;
            }
        }

        private static async Task RunAsync(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var presentaionAssembly = typeof(PresentaionLayerAssemblyRefrence).Assembly;
            builder.Services.AddControllers().AddApplicationPart(presentaionAssembly);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddServicesForApplicationLayer()
                            .AddDataBase(builder.Configuration)
                            .AddServicesForApi()
                            .AddHangFireConfig(builder.Configuration)
                            .AddFireBaseConfigurations(builder.Configuration,
                                                       builder.Environment);



            var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "Keys");
            builder.Services.AddDataProtection()
                            .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

            builder.Services.AddIdentityCore<User>()
                            .AddRoles<Role>()
                            .AddEntityFrameworkStores<NaqlahContext>()
                            .AddSignInManager()
                            .AddApiEndpoints();

            builder.Services.AddAuthentication()
                            .AddBearerToken(IdentityConstants.BearerScheme, opt =>
                            {
                                opt.BearerTokenExpiration = TimeSpan.FromMinutes(600);
                                opt.RefreshTokenExpiration = TimeSpan.FromMinutes(43200);
                            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
            });

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources")
                   .Configure<RequestLocalizationOptions>(options =>
                   {
                       var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar") };
                       foreach (var culture in supportedCultures)
                       {
                           culture.DateTimeFormat.Calendar = new GregorianCalendar();
                       }

                       options.DefaultRequestCulture = new RequestCulture(culture: "en-US",
                                                                          uiCulture: "en-US");
                       options.SupportedCultures = supportedCultures;
                       options.SupportedUICultures = supportedCultures;
                   });

            builder.Services.AddOpenApiDocument(document =>
            {
                document.Title = "Naqlah API";
            });

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddHangfireServer();

            // Add SignalR
            builder.Services.AddSignalR();


            var app = builder.Build();
            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            // Migration and seeding - skip during NSwag generation
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<NaqlahContext>();
                    if (context.Database.CanConnect())
                    {
                        context.Database.Migrate();
                        await SeedDefaultUsers(scope);
                        await NAQLAH.Server.SeedData.RolePermissionsSeed.SeedAdminRolePermissionsAsync(
                            scope.ServiceProvider.GetRequiredService<RoleManager<Role>>());
                    }
                }
            }
            catch (Exception)
            {
                // Ignore migration errors during NSwag generation or if database is not available
            }

            async Task SeedDefaultUsers(IServiceScope scope)
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

                // Create default user if not exists

                var defaultUser = await userManager.FindByNameAsync("admin@accflex.com");
                if (defaultUser == null)
                {
                    var user = new User
                    {
                        UserName = "admin@accflex.com",
                        PhoneNumber = "01029249892",
                        Email = "admin@accflex.com",
                        IsActive = true
                    };
                    var result = await userManager.CreateAsync(user, "Password500$$");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }


            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Enable CORS - must be before UseAuthentication and UseAuthorization
            app.UseCors("AllowAll");

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();


            //RecurringJob.AddOrUpdate<ProcessScheduledNotificationsJob>(
            //    "process-scheduled-notifications",
            //    job => job.ProcessAsync(),
            //    Cron.Minutely());

            app.UseMiddleware<ExceptionHandlingMiddleWare>();
            app.UseHttpsRedirection();

            app.MapIdentityApi<User>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<SessionInfoMiddleWare>();
            app.UseHangfireDashboard();

            RecurringJob.AddOrUpdate<Infrastructure.Jobs.CaptainIncompleteProfileReminderJob>(
                "captain-incomplete-profile-reminder",
                job => job.ExecuteAsync(),
                Cron.Daily);

            app.MapControllers();

            // Map SignalR Hub
            app.MapHub<Presentaion.Hubs.NotificationHub>("/NotificationHub");

            app.MapFallbackToFile("/index.html");

            app.Run();
        }

        private static void LogStartupFailure(Exception ex)
        {
            var message = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] Application failed to start.{Environment.NewLine}{ex}{Environment.NewLine}";
            Console.Error.WriteLine(message);

            try
            {
                var logsFolder = Path.Combine(AppContext.BaseDirectory, "Logs");
                Directory.CreateDirectory(logsFolder);
                File.AppendAllText(Path.Combine(logsFolder, "startup-failures.log"), message);
            }
            catch
            {
                // Never let logging mask the original startup failure.
            }
        }
    }
}
