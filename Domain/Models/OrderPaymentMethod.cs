using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class OrderPaymentMethod
    {
        private OrderPaymentMethod()
        {
            
        }
        public int OrderId { get;private set; }
        public int PaymentMethodId { get;private set; }
        public decimal Amount { get;private set; }
        public OrderPaymentStatus OrderPaymentStatus { get;private set; }
        public Order Order { get;private set; }
        public PaymentMethod PaymentMethod { get;private set; }

        public static OrderPaymentMethod Instance(int paymentMethodId, decimal amount)
        {
            return new OrderPaymentMethod
            {
                PaymentMethodId = paymentMethodId,
                Amount = amount,
                OrderPaymentStatus = OrderPaymentStatus.Pending
            };
        }
    }
}
