using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MainCategory
    {
        private MainCategory()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
            this._CategorySizes = new List<CategorySize>();
        }
        public int Id { get; private set; }
        public string ArabicName { get; private set; }
        public string EnglishName { get; private set; }
        private List<CategorySize> _CategorySizes { get; set; }
        public IReadOnlyList<CategorySize> CategorySizes
        {
            get
            {
                return _CategorySizes;
            }
            private set
            {
                _CategorySizes = (List<CategorySize>)value.ToList();
            }
        }

    }
}
