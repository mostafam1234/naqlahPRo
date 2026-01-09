using Domain.Enums;
using System;

namespace Application.Features.DeliveryManSection.NewRequests.Dtos
{
    public class GetDeliveryManRequestDetailsDto
    {
        public int DeliveryManId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime IdentityExpirationDate { get; set; }
        public DateTime DrivingLicenseExpirationDate { get; set; }
        public string DeliveryType { get; set; }
        public string DeliveryLicenseType { get; set; }
        public string State { get; set; }
        public string StateName { get; set; }
        
        // صور الهوية
        public string FrontIdentityImagePath { get; set; }
        public string BackIdentityImagePath { get; set; }
        
        // صور رخصة القيادة
        public string FrontDrivingLicenseImagePath { get; set; }
        public string BackDrivingLicenseImagePath { get; set; }
        
        // الصورة الشخصية
        public string PersonalImagePath { get; set; }
        
        // معلومات إضافية
        public bool Active { get; set; }
        public string AndroidDevice { get; set; }
        public string IosDevice { get; set; }
        
        // بيانات المستخدم المرتبط
        public int UserId { get; set; }
        public string Email { get; set; }
        
        // بيانات المركبة (إذا وجدت)
        public int? VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; }
        public string VehicleType { get; set; }
        public int? VehicleTypeId { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleModel { get; set; }
        public int? VehicleBrandId { get; set; }
        
        // صور المركبة
        public string VehicleFrontImagePath { get; set; }
        public string VehicleSideImagePath { get; set; }
        public string VehicleFrontLicenseImagePath { get; set; }
        public string VehicleBackLicenseImagePath { get; set; }
        public string VehicleFrontInsuranceImagePath { get; set; }
        public string VehicleBackInsuranceImagePath { get; set; }
    }
}