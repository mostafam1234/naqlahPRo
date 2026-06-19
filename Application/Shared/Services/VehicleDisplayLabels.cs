using Domain.Enums;

namespace Application.Shared.Services
{
    public static class VehicleDisplayLabels
    {
        public static string GetLoadCategoryName(VehicleLoadCategory category, int languageId = 1)
        {
            var isArabic = languageId == (int)Language.Arabic;

            return category switch
            {
                VehicleLoadCategory.Dina5Ton => isArabic ? "دينا حمولة 5 طن" : "Dina 5 ton",
                VehicleLoadCategory.HalfTransport3_5Ton => isArabic ? "نص نقل حمولة 3.5 طن" : "Half transport 3.5 ton",
                VehicleLoadCategory.Truck5TonInsulated => isArabic ? "شاحنة حمولة 5 طن مصندقة" : "Insulated truck 5 ton",
                VehicleLoadCategory.HalfTransport3_5TonInsulatedAc => isArabic
                    ? "نص نقل حمولة 3.5 طن مصندقة مكيفة"
                    : "Insulated AC half transport 3.5 ton",
                _ => isArabic ? "غير محدد" : "Not specified"
            };
        }

        public static IReadOnlyList<VehicleLoadCategory> AllLoadCategories { get; } =
            new[]
            {
                VehicleLoadCategory.Dina5Ton,
                VehicleLoadCategory.HalfTransport3_5Ton,
                VehicleLoadCategory.Truck5TonInsulated,
                VehicleLoadCategory.HalfTransport3_5TonInsulatedAc
            };
    }
}
