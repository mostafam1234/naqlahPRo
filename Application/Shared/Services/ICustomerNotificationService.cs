using Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Shared.Services
{
    public interface ICustomerNotificationService
    {
        Task PrepareAsync(
            int customerId,
            int? orderId,
            string arabicTitle,
            string englishTitle,
            string arabicMessage,
            string englishMessage,
            NotificationType notificationType,
            CancellationToken cancellationToken = default);
    }
}
