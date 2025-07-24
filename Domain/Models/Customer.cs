using CSharpFunctionalExtensions;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Customer
    {
        public Customer()
        {
            this.PhoneNumber = string.Empty;
            this.AndriodDevice = string.Empty;
            this.IosDevice = string.Empty;
        }
        public int Id { get;private set; }
        public int UserId { get;private set; }
        public string PhoneNumber { get;private set; }
        public CustomerType CustomerType { get;private set; }
        public Individual? Individual { get;private set; }
        public EstablishMent? EstablishMent { get;private set; }
        public string AndriodDevice { get;private set; }
        public string IosDevice { get;private set; }
        public User User { get; set; }
    }
}
