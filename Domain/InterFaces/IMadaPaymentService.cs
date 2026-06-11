using Domain.Shared;

namespace Domain.InterFaces
{
    public interface IMadaPaymentService
    {
        Task<MadaCheckoutResult> InitiateCheckoutAsync(
            int orderId,
            decimal amount,
            string currency = "SAR",
            CancellationToken cancellationToken = default);
    }
}
