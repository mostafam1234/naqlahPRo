using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.LogIn.Dtos
{
    public class DeliveryManInfoDto
    {
        public DeliveryManInfoDto()
        {
            this.Name = string.Empty;
            this.PhoneNumber = string.Empty;
            this.PersonalImagePath = string.Empty;
            this.ChatUrl = string.Empty;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string PersonalImagePath { get; set; }
        public bool Active { get; set; }
        public string ChatUrl { get; set; }
    }
}
