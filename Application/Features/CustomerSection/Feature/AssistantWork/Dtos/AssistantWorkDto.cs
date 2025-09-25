using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.AssistantWork.Dtos
{
    public class AssistantWorkDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public AssistantWorkDto()
        {
            Name = string.Empty;
        }
    }
}