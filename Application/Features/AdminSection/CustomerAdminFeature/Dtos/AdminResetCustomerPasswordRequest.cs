namespace Application.Features.AdminSection.CustomerAdminFeature.Dtos
{
    public sealed class AdminResetCustomerPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
