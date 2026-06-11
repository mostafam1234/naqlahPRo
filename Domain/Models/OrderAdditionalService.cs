using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class OrderAdditionalService
    {
        private OrderAdditionalService()
        {
        }

        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public int AdditionalServiceId { get; private set; }
        public int Quantity { get; private set; }
        public decimal Amount { get; private set; }
        public AdditionalService AdditionalService { get; private set; }

        public static Result<OrderAdditionalService> Create(int additionalServiceId, int quantity, decimal unitPrice)
        {
            if (quantity <= 0)
                return Result.Failure<OrderAdditionalService>("Quantity must be greater than zero");

            return new OrderAdditionalService
            {
                AdditionalServiceId = additionalServiceId,
                Quantity = quantity,
                Amount = unitPrice * quantity
            };
        }
    }
}
