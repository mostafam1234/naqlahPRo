using Domain.Enums;

namespace Application.Shared.Services
{
    public static class DeliveryManDisplayLabels
    {
        public static string GetDeliveryStateName(DeliveryRequesState state, int languageId = 1)
        {
            var isArabic = languageId == (int)Language.Arabic;

            return state switch
            {
                DeliveryRequesState.New => isArabic ? "جديد" : "New",
                DeliveryRequesState.Approved => isArabic ? "موافق عليه" : "Approved",
                DeliveryRequesState.Rejected => isArabic ? "مرفوض" : "Rejected",
                DeliveryRequesState.Blocked => isArabic ? "محظور" : "Blocked",
                DeliveryRequesState.Suspended => isArabic ? "معلق" : "Suspended",
                _ => isArabic ? "غير محدد" : "Not specified"
            };
        }

        public static string GetActiveStatusName(bool active, int languageId = 1)
        {
            var isArabic = languageId == (int)Language.Arabic;
            return active
                ? (isArabic ? "نشط" : "Active")
                : (isArabic ? "غير نشط" : "Inactive");
        }
    }
}
