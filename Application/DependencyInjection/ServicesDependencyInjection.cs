using Application.Behaviours;
using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Exporters;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection
{
    public static class ServicesDependencyInjection
    {
        public static IServiceCollection AddServicesForApplicationLayer(this IServiceCollection services)
        {

            var currentAssembly = typeof(ServicesDependencyInjection).Assembly;

            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(currentAssembly);
            });
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ResultLocalizationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehaviour<,>));
            services.AddValidatorsFromAssembly(currentAssembly);

            // Register Notification Service
            services.AddScoped<Application.Shared.Services.INotificationService, Application.Shared.Services.NotificationService>();
            services.AddScoped<Application.Features.AdminSection.DeliveryManFeature.Queries.CaptainControlOrdersService>();

            // Backup / Excel export - module exporters
            services.AddScoped<IModuleExporter, OrdersExcelExporter>();
            services.AddScoped<IModuleExporter, OrderPackagesExcelExporter>();
            services.AddScoped<IModuleExporter, VehiclesExcelExporter>();
            services.AddScoped<IModuleExporter, SystemUsersExcelExporter>();
            services.AddScoped<IModuleExporter, DeliveryMenExcelExporter>();
            services.AddScoped<IModuleExporter, MainCategoriesExcelExporter>();
            services.AddScoped<IModuleExporter, WalletTransactionsExcelExporter>();
            services.AddScoped<IModuleExporter, ComplainsExcelExporter>();
            services.AddScoped<IModuleExporter, SuggestionsExcelExporter>();
            services.AddScoped<IModuleExporter, NotificationsExcelExporter>();
            services.AddScoped<IModuleExporter, RegionsExcelExporter>();
            services.AddScoped<IModuleExporter, CitiesExcelExporter>();
            services.AddScoped<IModuleExporter, NeighborhoodsExcelExporter>();
            services.AddScoped<IModuleExporter, AssistantWorksExcelExporter>();

            return services;
        }
    }
}
