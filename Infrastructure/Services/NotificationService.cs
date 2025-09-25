using Domain.InterFaces;
using Domain.Shared;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            this.logger = logger;
        }
        public async Task SendNotificationAsyncToMultipleDevices(NotificationBodyForMultipleDevices notificationBody)
        {
            try
            {
                var deviceIds = notificationBody.FireBaseTokens.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                if (deviceIds.Count == 0)
                {
                    return;
                }

                var message = new MulticastMessage()
                {
                    Tokens = deviceIds,
                    Notification = new Notification
                    {
                        Title = notificationBody.Title,
                        Body = notificationBody.Body
                    },
                    Data = notificationBody.PayLoad,
                    Android = new AndroidConfig
                    {
                        Notification = new AndroidNotification
                        {
                            ChannelId = "1",
                            ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                            DefaultSound = true,
                            Priority = NotificationPriority.HIGH,
                            EventTimestamp = DateTime.UtcNow
                        }
                    }
                };


                var result = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                logger.LogInformation("Firebase success count: {0} ,Firebase failed count: {1}", result.SuccessCount, result.FailureCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Sending Notifications");
                return;
            }


        }



        public async Task SendNotificationForSingleDevice(NotificationBody notificationBody)
        {
            try
            {
                var message = new Message()
                {
                    Token = notificationBody.FireBaseToken,
                    Notification = new Notification
                    {
                        Title = notificationBody.Title,
                        Body = notificationBody.Body
                    },
                    Data = notificationBody.PayLoad,
                    Android = new AndroidConfig
                    {
                        Notification = new AndroidNotification
                        {
                            ChannelId = "1",
                            ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                            DefaultSound = true,
                            Priority = NotificationPriority.HIGH,
                            EventTimestamp = DateTime.UtcNow
                        }
                    }
                };



                var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                logger.LogInformation("Firebase result is {0} for device {1}", result, notificationBody.FireBaseToken);
            }

            catch (FirebaseMessagingException fireBaseEx)
            {
                logger.LogError(fireBaseEx, "Error in Sending Notifications");
                return;
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Sending Notifications");
                return;
            }


        }



    }
}
