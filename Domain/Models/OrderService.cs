using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class OrderService
    {
        private OrderService()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
        }
        public int Id { get;private set; }
        public int OrderId { get; set; }
        public int WorkId { get;private set; }
        public string ArabicName { get;private set; }
        public string EnglishName { get;private set; }
        public decimal Amount { get;private set; }
        public AssistanWork AssistanWork { get;private set; }

        public static OrderService Instance(int workId,
                                            decimal amount,
                                            string arabicName,
                                            string englishName)
        {
            var orderService = new OrderService
            {
                WorkId = workId,
                Amount = amount,
                ArabicName = arabicName,
                EnglishName = englishName
            };
            return orderService;
        } 
        

    }
}
