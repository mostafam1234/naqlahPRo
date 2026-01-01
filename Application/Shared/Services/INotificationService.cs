using Domain.Enums;
using Domain.Models;
using System.Threading.Tasks;

namespace Application.Shared.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(
            string arabicTitle,
            string englishTitle,
            string arabicMessage,
            string englishMessage,
            NotificationType notificationType,
            int? orderId = null,
            int? userId = null);
    }
}

