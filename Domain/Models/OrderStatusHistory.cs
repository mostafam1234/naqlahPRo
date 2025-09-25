using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class OrderStatusHistory
    {
        private OrderStatusHistory()
        {

        }
        public int Id { get; private set; }
        public DateTime CreationDate { get; private set; }
        public int OrderId { get; private set; }
        public OrderStatus OrderStatus { get; private set; }

        public static OrderStatusHistory Create(OrderStatus orderStatus,DateTime nowDate)
        {
            return new OrderStatusHistory
            {
                OrderStatus = orderStatus,
                CreationDate = nowDate
            };
        }
    }
}
