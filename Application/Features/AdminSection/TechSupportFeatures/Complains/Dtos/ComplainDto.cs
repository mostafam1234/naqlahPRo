using System;

namespace Application.Features.AdminSection.TechSupportFeatures.Complains.Dtos
{
    public class ComplainDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobileNumber { get; set; }
        public string CustomerAddress { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
    }
}

