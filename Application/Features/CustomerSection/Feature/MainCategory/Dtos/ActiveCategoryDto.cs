using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.MainCategory.Dtos
{
    public class ActiveCategoryDto
    {
        public ActiveCategoryDto()
        {
            this.Name = string.Empty;
        }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
