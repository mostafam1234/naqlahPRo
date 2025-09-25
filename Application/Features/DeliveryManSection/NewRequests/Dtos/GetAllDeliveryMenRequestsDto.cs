using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.NewRequests.Dtos
{
    public class GetAllDeliveryMenRequestsDto
    {
        public int DeliveryManId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string PersonalImagePath { get; set; }
        public string DeliveryType { get; set; }
        public string State { get; set; }
    }
}
