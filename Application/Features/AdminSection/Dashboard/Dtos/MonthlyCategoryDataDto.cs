using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.Dashboard.Dtos
{
    public class MonthlyCategoryDataDto
    {
        public string MonthName { get; set; } = string.Empty;
        public int MonthNumber { get; set; }
        public int Year { get; set; }
        public List<CategoryOrderCountDto> Categories { get; set; } = new List<CategoryOrderCountDto>();
    }
}

