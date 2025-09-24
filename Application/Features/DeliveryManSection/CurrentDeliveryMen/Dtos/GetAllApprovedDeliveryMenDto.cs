using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.CurrentDeliveryMen.Dtos
{
    public class GetAllApprovedDeliveryMenDto
    {
        public int DeliveryManId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string PersonalImagePath { get; set; }
        public string DeliveryType { get; set; }
        public int State { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Active { get; set; }
    }
}