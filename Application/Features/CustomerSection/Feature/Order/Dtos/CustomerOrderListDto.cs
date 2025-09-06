using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class CustomerOrderListDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        public decimal Total { get; set; }
        public string DeliveryManName { get; set; }
    }

    public class PagedCustomerOrdersDto
    {
        public List<CustomerOrderListDto> Orders { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public PagedCustomerOrdersDto()
        {
            Orders = new List<CustomerOrderListDto>();
        }
    }

    public class CustomerOrdersQueryRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public OrderStatus? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}