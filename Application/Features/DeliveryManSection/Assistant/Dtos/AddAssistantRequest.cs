using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Assistant.Dtos
{
    public class AddAssistantRequest
    {
        public AddAssistantRequest()
        {
            this.Name = string.Empty;
            this.Phone = string.Empty;
            this.Address = string.Empty;
            this.IdentityNumber = string.Empty;
            this.FrontIdentityImage = string.Empty;
            this.BackIdentityImage = string.Empty;
            this.IdentityExpirationDate = string.Empty;
        }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string IdentityNumber { get; set; }
        public string FrontIdentityImage { get; set; }
        public string BackIdentityImage { get; set; }
        public string IdentityExpirationDate { get; set; }
        public int MaidTypeId { get; set; }
    }
}
