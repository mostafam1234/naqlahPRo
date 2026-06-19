using Domain.Enums;

namespace Application.Shared.Services
{
    public static class OrderDisplayLabels
    {
        public static string GetOrderStatusName(OrderStatus status, int languageId)
        {
            var isArabic = languageId == (int)Language.Arabic;

            return status switch
            {
                OrderStatus.Pending => isArabic ? "معلقة" : "Pending",
                OrderStatus.Assigned => isArabic ? "منسوبة الى مندوب" : "Assigned to delivery man",
                OrderStatus.Cancelled => isArabic ? "ملغية" : "Cancelled",
                OrderStatus.Completed => isArabic ? "مكتملة" : "Completed",
                OrderStatus.ConfirmedGoingToPickup => isArabic ? "تم تأكيد الذهاب لالتقاط الشحنة" : "Confirmed going to pickup shipment",
                OrderStatus.PickedUpFromDeliveryMan => isArabic ? "التقاط الطلب من المندوب" : "Pickup from delivery man",
                _ => isArabic ? "غير محدد" : "Not specified"
            };
        }

        public static string GetCustomerTypeName(CustomerType customerType, int languageId)
        {
            var isArabic = languageId == (int)Language.Arabic;

            return customerType switch
            {
                CustomerType.Individual => isArabic ? "فرد" : "Individual",
                CustomerType.Establishment => isArabic ? "شركة / مؤسسة" : "Establishment",
                _ => isArabic ? "غير محدد" : "Not specified"
            };
        }

        public static string GetOrderTypeName(OrderType orderType, int languageId)
        {
            var isArabic = languageId == (int)Language.Arabic;

            return orderType switch
            {
                OrderType.SingleWayPoints => isArabic ? "نقطة واحدة" : "Single way point",
                OrderType.MultiWayPoints => isArabic ? "عدة نقاط" : "Multiple way points",
                OrderType.BackAndForth => isArabic ? "ذهاب وعودة" : "Back and forth",
                _ => isArabic ? "غير محدد" : "Not specified"
            };
        }

        public static string GetWayPointStatusName(OrderWayPointsStatus status, int languageId)
        {
            var isArabic = languageId == (int)Language.Arabic;

            return status switch
            {
                OrderWayPointsStatus.Pending => isArabic ? "في الانتظار" : "Pending",
                OrderWayPointsStatus.PickedUp => isArabic ? "تم الوصول الى نقطة الإستلام" : "Picked up",
                OrderWayPointsStatus.Completed => isArabic ? "مكتمل" : "Completed",
                _ => isArabic ? "غير محدد" : "Not specified"
            };
        }
    }
}
