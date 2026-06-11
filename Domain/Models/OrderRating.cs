using CSharpFunctionalExtensions;

namespace Domain.Models
{
    public class OrderRating
    {
        private OrderRating()
        {
            Comment = string.Empty;
        }

        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public int CustomerId { get; private set; }
        public int? DeliveryManId { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }
        public DateTime CreatedDate { get; private set; }

        public static Result<OrderRating> Instance(
            int orderId,
            int customerId,
            int? deliveryManId,
            int rating,
            string? comment,
            DateTime createdDate)
        {
            if (rating < 1 || rating > 5)
                return Result.Failure<OrderRating>("Rating must be between 1 and 5.");

            return new OrderRating
            {
                OrderId = orderId,
                CustomerId = customerId,
                DeliveryManId = deliveryManId,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
                CreatedDate = createdDate
            };
        }
    }
}
