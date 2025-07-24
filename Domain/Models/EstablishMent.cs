using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class EstablishMent
    {
        public EstablishMent()
        {
            this.Name = string.Empty;
            this.MobileNumber = string.Empty;
            this.RecoredImagePath = string.Empty;
            this.TaxRegistrationImagePath = string.Empty;
            this.TaxRegistrationImagePath = string.Empty;
            this.Address = string.Empty;
        }
        public int Id { get;private set; }
        public int CustomerId { get;private set; }
        public string Name { get;private set; }
        public string MobileNumber { get;private set; }
        public string RecoredImagePath { get;private set; }
        public string TaxRegistrationNumber { get;private set; }
        public string TaxRegistrationImagePath { get;private set; }
        public string Address { get;private set; }
        public Customer Customer { get; set; }
        public EstablishMentRepresentitive EstablishMentRepresentitive { get;private set; }

        public static Result<EstablishMent> Instance(string name,
                                                     string phoneNumber,
                                                     string recordImagePath,
                                                     string taxRegestrationNumber,
                                                     string taxRegestrationImage,
                                                     string adress,
                                                     EstablishMentRepresentitive establishMentRepresentitive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<EstablishMent>("Name is Required");
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Result.Failure<EstablishMent>("PhoneNumber is Required");
            }

            if (string.IsNullOrWhiteSpace(recordImagePath))
            {
                return Result.Failure<EstablishMent>("Record Image is Required");
            }

            if (string.IsNullOrWhiteSpace(taxRegestrationImage))
            {
                return Result.Failure<EstablishMent>("Tax Registeration Number  is Required");
            }

            if (string.IsNullOrWhiteSpace(taxRegestrationImage))
            {
                return Result.Failure<EstablishMent>("Tax Registeration Image is Required");
            }

            if (string.IsNullOrWhiteSpace(adress))
            {
                return Result.Failure<EstablishMent>("Address is Required");
            }



            return new EstablishMent
            {
                Name = name,
                MobileNumber = phoneNumber,
                Address = adress,
                RecoredImagePath = recordImagePath,
                TaxRegistrationImagePath = taxRegestrationImage,
                TaxRegistrationNumber = taxRegestrationNumber,
                EstablishMentRepresentitive = establishMentRepresentitive
            };
        }
    }
}
