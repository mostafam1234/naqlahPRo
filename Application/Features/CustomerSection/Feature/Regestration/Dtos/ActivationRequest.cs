using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Regestration.Dtos
{
    public class ActivationRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string ActiveCode { get; set; } = string.Empty;
    }
}
