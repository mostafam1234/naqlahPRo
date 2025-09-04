using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class OrderDetails
    {
        public OrderDetails()
        {
            this.ArabicCategoryName = string.Empty;
            this.EnglishCategoryName = string.Empty;
        }
        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public int MainCategoryId { get; private set; }
        public string ArabicCategoryName { get; private set; }
        public string EnglishCategoryName { get; private set; }
        public MainCategory MainCategory { get; private set; }

        public static OrderDetails Create(int mainCategoryId,
                                          string arabicCategoryName,
                                          string englishCategoryName)
        {
            return new OrderDetails
            {
                MainCategoryId = mainCategoryId,
                ArabicCategoryName = arabicCategoryName,
                EnglishCategoryName = englishCategoryName
            };
        }
    }
}
