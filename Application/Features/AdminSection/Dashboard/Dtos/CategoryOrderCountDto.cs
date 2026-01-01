using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.Dashboard.Dtos
{
    public class CategoryOrderCountDto
    {
        public int MainCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
    }
}

