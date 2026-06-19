using Application.Shared.Services;
using Domain.InterFaces;
using Domain.Models;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs
{
    public class CaptainIncompleteProfileReminderJob
    {
        private const int ReminderDaysAfterRegistration = 30;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<CaptainIncompleteProfileReminderJob> logger;

        public CaptainIncompleteProfileReminderJob(
            IServiceScopeFactory scopeFactory,
            ILogger<CaptainIncompleteProfileReminderJob> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        public async Task ExecuteAsync()
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<INaqlahContext>();
            var pushService = scope.ServiceProvider.GetRequiredService<Domain.InterFaces.INotificationService>();

            var cutoff = DateTime.UtcNow.AddDays(-ReminderDaysAfterRegistration);

            var deliveryMen = await context.DeliveryMen
                .Include(x => x.Vehicle!)
                    .ThenInclude(v => v.Resident)
                .Include(x => x.Vehicle!)
                    .ThenInclude(v => v.Company)
                .Include(x => x.Vehicle!)
                    .ThenInclude(v => v.Renter)
                .Where(x => x.HasIncompleteRegistration
                    && x.RegisteredAt <= cutoff
                    && x.IncompleteProfileReminderSentAt == null)
                .ToListAsync();

            foreach (var deliveryMan in deliveryMen)
            {
                try
                {
                    DeliveryManProfileCompleteness.ApplyCompletenessState(deliveryMan);
                    if (!deliveryMan.HasIncompleteRegistration)
                        continue;

                    var missingKeys = DeliveryManProfileCompleteness.GetMissingFieldKeys(deliveryMan);
                    var bodyAr = DeliveryManProfileCompleteness.BuildNotificationBody(missingKeys, arabic: true);
                    var titleAr = DeliveryManProfileCompleteness.GetNotificationTitle(arabic: true);

                    var notification = new CaptainNotification
                    {
                        DeliveryManId = deliveryMan.Id,
                        Title = titleAr,
                        Body = bodyAr,
                        MissingFieldsJson = deliveryMan.MissingProfileFieldsJson,
                        LegalDisclaimer = DeliveryManProfileCompleteness.LegalDisclaimerAr,
                        IsPushSent = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await context.CaptainNotifications.AddAsync(notification);

                    var tokens = new[] { deliveryMan.AndriodDevice, deliveryMan.IosDevice }
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();

                    if (tokens.Count > 0)
                    {
                        await pushService.SendNotificationAsyncToMultipleDevices(
                            new NotificationBodyForMultipleDevices
                            {
                                Title = titleAr,
                                Body = bodyAr,
                                FireBaseTokens = tokens,
                                PayLoad = new Dictionary<string, string>
                                {
                                    ["type"] = "incompleteCaptainProfile",
                                    ["deliveryManId"] = deliveryMan.Id.ToString(),
                                    ["missingFields"] = deliveryMan.MissingProfileFieldsJson
                                }
                            });
                        notification.IsPushSent = true;
                    }

                    deliveryMan.MarkIncompleteProfileReminderSent();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed incomplete-profile reminder for delivery man {Id}", deliveryMan.Id);
                }
            }

            await context.SaveChangesAsyncWithResult();
        }
    }
}
