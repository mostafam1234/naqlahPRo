using System;

namespace Domain.Models
{
    public class Complain
    {
        private Complain()
        {
            this.CustomerName = string.Empty;
            this.CustomerMobileNumber = string.Empty;
            this.CustomerAddress = string.Empty;
            this.Description = string.Empty;
        }

        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public string CustomerName { get; private set; }
        public string CustomerMobileNumber { get; private set; }
        public string CustomerAddress { get; private set; }
        public string Description { get; private set; }
        public DateTime CreationDate { get; private set; }
    }
}

