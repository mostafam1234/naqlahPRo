using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs
{
    /// <summary>
    /// Sends a reminder to the assigned driver ~2 hours before a scheduled pickup when the driver has
    /// not yet confirmed going to pickup. Runs frequently (e.g. every 15 minutes).
    /// </summary>
    public class ScheduledPickupReminderJob
    {
        private const int ReminderWindowHours = 2;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<ScheduledPickupReminderJob> logger;

        public ScheduledPickupReminderJob(
            IServiceScopeFactory scopeFactory,
            ILogger<ScheduledPickupReminderJob> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        public async Task ExecuteAsync()
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<INaqlahContext>();
            var pushService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.UtcNow;
            var windowEnd = now.AddHours(ReminderWindowHours);

            var orders = await context.Orders
                .AsTracking()
                .Where(o => o.IsScheduled
                    && o.DeliveryManId != null
                    && o.OrderStatus == OrderStatus.Assigned
                    && !o.DriverConfirmedGoingToPickup
                    && o.PickupReminderSentAt == null
                    && o.ExpectedPickUpTime != null
                    && o.ExpectedPickUpTime > now
                    && o.ExpectedPickUpTime <= windowEnd)
                .ToListAsync();

            if (orders.Count == 0)
            {
                return;
            }

            var deliveryManIds = orders.Select(o => o.DeliveryManId!.Value).Distinct().ToList();
            var deliveryMen = await context.DeliveryMen
                .Where(d => deliveryManIds.Contains(d.Id))
                .ToListAsync();

            foreach (var order in orders)
            {
                try
                {
                    var deliveryMan = deliveryMen.FirstOrDefault(d => d.Id == order.DeliveryManId);
                    if (deliveryMan is null)
                    {
                        continue;
                    }

                    var titleAr = "تذكير بموعد الالتقاط";
                    var bodyAr = $"لديك التقاط مجدول للطلب رقم {order.OrderNumber} خلال ساعتين، يرجى تأكيد التوجه";

                    await context.CaptainNotifications.AddAsync(new CaptainNotification
                    {
                        DeliveryManId = deliveryMan.Id,
                        Title = titleAr,
                        Body = bodyAr,
                        MissingFieldsJson = "[]",
                        LegalDisclaimer = string.Empty,
                        Type = "pickupReminder",
                        OrderId = order.Id,
                        IsPushSent = false,
                        CreatedAt = now
                    });

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
                                    ["type"] = "pickupReminder",
                                    ["orderId"] = order.Id.ToString(),
                                    ["orderNumber"] = order.OrderNumber
                                }
                            });
                    }

                    order.MarkPickupReminderSent(now);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed scheduled-pickup reminder for order {OrderId}", order.Id);
                }
            }

            await context.SaveChangesAsyncWithResult();
        }
    }
}
