using Application.DependencyInjection;
using Domain.Models;
using Hangfire;
using Infrastructure;
using Infrastructure.HangFireDepencies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Presentaion;

namespace NAQLAH.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
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
                            .AddHangFireConfig(builder.Configuration);



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

            builder.Services.AddOpenApiDocument(document =>
            {
                document.Title = "Naqlah API";
            });

            builder.Services.AddHangfireServer();


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<NaqlahContext>().Database.Migrate();
                await SeedDefaultUsers(scope);
            }
            async Task SeedDefaultUsers(IServiceScope scope)
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

                // Create default user if not exists

                var defaultUser = await userManager.FindByNameAsync("admin");
                if (defaultUser == null)
                {
                    var user = new User
                    {
                        UserName = "admin",
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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapIdentityApi<User>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHangfireDashboard();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
