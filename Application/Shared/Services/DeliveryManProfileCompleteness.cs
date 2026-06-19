using Domain.Enums;
using Domain.Models;
using System.Text.Json;

namespace Application.Shared.Services
{
    public static class DeliveryManProfileCompleteness
    {
        public const string LegalDisclaimerAr =
            "يُقر المستخدم بأن إدخال البيانات الناقصة مسؤوليته، وأن المنصة غير مسؤولة عن أي أضرار ناتجة عن عدم اكتمال بيانات التسجيل بعد التنبيه.";

        public const string LegalDisclaimerEn =
            "The user acknowledges that completing missing data is their responsibility and the platform is not liable for damages resulting from incomplete registration after notification.";

        public static IReadOnlyList<string> GetMissingFieldKeys(DeliveryMan deliveryMan)
        {
            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(deliveryMan.FullName))
                missing.Add("fullName");
            if (string.IsNullOrWhiteSpace(deliveryMan.PhoneNumber))
                missing.Add("phoneNumber");
            if (string.IsNullOrWhiteSpace(deliveryMan.IdentityNumber))
                missing.Add("identityNumber");
            if (!deliveryMan.BirthDate.HasValue)
                missing.Add("birthDate");
            if (deliveryMan.DeliveryLicenseType == default)
                missing.Add("deliveryLicenseType");
            if (string.IsNullOrWhiteSpace(deliveryMan.FrontIdenitytImagePath))
                missing.Add("frontIdentityImage");
            if (string.IsNullOrWhiteSpace(deliveryMan.FrontDrivingLicenseImagePath))
                missing.Add("frontDrivingLicenseImage");

            // Optional personal fields (tracked for 30-day reminder)
            if (!deliveryMan.IdentityExpirationDate.HasValue)
                missing.Add("identityExpirationDate");
            if (!deliveryMan.DrivingLicenseExpirationDate.HasValue)
                missing.Add("drivingLicenseExpirationDate");
            if (string.IsNullOrWhiteSpace(deliveryMan.Address))
                missing.Add("address");
            if (string.IsNullOrWhiteSpace(deliveryMan.PersonalImagePath))
                missing.Add("personalImage");

            var vehicle = deliveryMan.Vehicle;
            if (vehicle is null)
            {
                missing.AddRange(new[]
                {
                    "vehicleType", "vehicleBrand", "vehiclePlateNumber", "vehicleOwnerType",
                    "vehicleOwnerName", "vehicleFrontImage", "vehicleSideImage", "vehicleRegistrationImage",
                    "ownerIdentityImage"
                });
                return missing;
            }

            if (vehicle.VehicleTypeId <= 0)
                missing.Add("vehicleType");
            if (vehicle.VehicleBrandId <= 0)
                missing.Add("vehicleBrand");
            if (string.IsNullOrWhiteSpace(vehicle.LicensePlateNumber))
                missing.Add("vehiclePlateNumber");
            if (vehicle.VehicleOwnerType == default)
                missing.Add("vehicleOwnerType");
            if (string.IsNullOrWhiteSpace(vehicle.FrontImagePath))
                missing.Add("vehicleFrontImage");
            if (string.IsNullOrWhiteSpace(vehicle.SideImagePath))
                missing.Add("vehicleSideImage");
            if (string.IsNullOrWhiteSpace(vehicle.FrontLicenseImagePath))
                missing.Add("vehicleRegistrationImage");

            if (!vehicle.LicenseExpirationDate.HasValue)
                missing.Add("vehicleLicenseExpirationDate");
            if (!vehicle.InSuranceExpirationDate.HasValue)
                missing.Add("vehicleInsuranceExpirationDate");
            if (string.IsNullOrWhiteSpace(vehicle.FrontInsuranceImagePath))
                missing.Add("vehicleInsuranceImage");

            switch (vehicle.VehicleOwnerType)
            {
                case VehicleOwnerType.Resident:
                    if (vehicle.Resident is null || string.IsNullOrWhiteSpace(vehicle.Resident.CitizenName))
                        missing.Add("vehicleOwnerName");
                    if (vehicle.Resident is null || string.IsNullOrWhiteSpace(vehicle.Resident.FrontIdentityImagePath))
                        missing.Add("ownerIdentityImage");
                    break;
                case VehicleOwnerType.Company:
                    if (vehicle.Company is null || string.IsNullOrWhiteSpace(vehicle.Company.CompanyName))
                        missing.Add("vehicleOwnerName");
                    if (vehicle.Company is null || string.IsNullOrWhiteSpace(vehicle.Company.RecordImagePath))
                        missing.Add("commercialRecordImage");
                    break;
                case VehicleOwnerType.Renter:
                    if (vehicle.Renter is null || string.IsNullOrWhiteSpace(vehicle.Renter.CitizenName))
                        missing.Add("vehicleOwnerName");
                    if (vehicle.Renter is null || string.IsNullOrWhiteSpace(vehicle.Renter.FrontIdentityImagePath))
                        missing.Add("ownerIdentityImage");
                    if (vehicle.Renter is null || string.IsNullOrWhiteSpace(vehicle.Renter.RentContractImagePath))
                        missing.Add("rentContractImage");
                    break;
            }

            return missing;
        }

        public static bool HasIncompleteRegistration(DeliveryMan deliveryMan) =>
            GetMissingFieldKeys(deliveryMan).Count > 0;

        public static void ApplyCompletenessState(DeliveryMan deliveryMan)
        {
            var missing = GetMissingFieldKeys(deliveryMan);
            deliveryMan.SetProfileCompleteness(missing.Count > 0, JsonSerializer.Serialize(missing));
        }

        public static string BuildNotificationBody(IReadOnlyList<string> missingKeys, bool arabic)
        {
            var labels = missingKeys.Select(k => GetFieldLabel(k, arabic)).Distinct().ToList();
            var list = string.Join(arabic ? "، " : ", ", labels);
            return arabic
                ? $"يرجى إكمال بيانات التسجيل الناقصة: {list}"
                : $"Please complete your missing registration data: {list}";
        }

        public static string GetNotificationTitle(bool arabic) =>
            arabic ? "بيانات تسجيل ناقصة" : "Incomplete registration data";

        private static string GetFieldLabel(string key, bool arabic) => key switch
        {
            "fullName" => arabic ? "اسم السائق" : "Driver name",
            "phoneNumber" => arabic ? "رقم الهاتف" : "Phone number",
            "identityNumber" => arabic ? "رقم الهوية" : "Identity number",
            "birthDate" => arabic ? "تاريخ الميلاد" : "Birth date",
            "deliveryLicenseType" => arabic ? "نوع رخصة القيادة" : "License type",
            "frontIdentityImage" => arabic ? "صورة الهوية (أمامية)" : "Identity photo (front)",
            "frontDrivingLicenseImage" => arabic ? "صورة رخصة القيادة" : "Driving license photo",
            "identityExpirationDate" => arabic ? "تاريخ انتهاء الهوية" : "Identity expiry date",
            "drivingLicenseExpirationDate" => arabic ? "تاريخ انتهاء رخصة القيادة" : "License expiry date",
            "address" => arabic ? "العنوان" : "Address",
            "personalImage" => arabic ? "الصورة الشخصية" : "Personal photo",
            "vehicleType" => arabic ? "نوع المركبة" : "Vehicle type",
            "vehicleBrand" => arabic ? "ماركة المركبة" : "Vehicle brand",
            "vehiclePlateNumber" => arabic ? "رقم اللوحة" : "Plate number",
            "vehicleOwnerType" => arabic ? "نوع مالك المركبة" : "Owner type",
            "vehicleOwnerName" => arabic ? "اسم مالك المركبة" : "Owner name",
            "vehicleFrontImage" => arabic ? "صورة أمامية للمركبة" : "Vehicle front photo",
            "vehicleSideImage" => arabic ? "صورة جانبية للمركبة" : "Vehicle side photo",
            "vehicleRegistrationImage" => arabic ? "صورة رخصة السير (الاستمارة)" : "Vehicle registration photo",
            "vehicleLicenseExpirationDate" => arabic ? "تاريخ انتهاء رخصة السير" : "Registration expiry",
            "vehicleInsuranceExpirationDate" => arabic ? "تاريخ انتهاء التأمين" : "Insurance expiry",
            "vehicleInsuranceImage" => arabic ? "صورة تأمين المركبة" : "Insurance photo",
            "ownerIdentityImage" => arabic ? "صورة هوية مالك المركبة" : "Owner identity photo",
            "commercialRecordImage" => arabic ? "صورة السجل التجاري" : "Commercial register photo",
            "rentContractImage" => arabic ? "وثيقة استئجار المركبة" : "Rent contract document",
            _ => key
        };
    }
}
