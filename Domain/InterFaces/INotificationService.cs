using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InterFaces
{
    public interface INotificationService
    {
        Task SendNotificationAsyncToMultipleDevices(NotificationBodyForMultipleDevices notificationBody);

        Task SendNotificationForSingleDevice(NotificationBody notificationBody);
    }
}
