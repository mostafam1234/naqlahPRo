﻿using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Company
    {
        public Company()
        {
            this.CompanyName = string.Empty;
            this.TaxNumber = string.Empty;
            this.CommercialRecordNumber = string.Empty;
            this.RecordImagePath = string.Empty;
            this.BankAccountNumber = string.Empty;
            this.TaxCertificateImagePath = string.Empty;
        }
        [Key]
        public int DeliveryVehicleId { get;private set; }
        public string CompanyName { get;private set; }
        public string CommercialRecordNumber { get;private set; }
        public string RecordImagePath { get;private set; }
        public string TaxNumber { get;private set; }
        public string TaxCertificateImagePath { get; private set; }
        public string BankAccountNumber { get;private set; }


        public static Result<Company>Instance(string companName,
                                              string commercialRecordNumbeer,
                                              string recordImagePath,
                                              string taxNumber,
                                              string taxCertificateImagePath,
                                              string bankAccounNumber)
        {
            var company = new Company
            {
                CompanyName = companName,
                CommercialRecordNumber = commercialRecordNumbeer,
                RecordImagePath = recordImagePath,
                TaxNumber = taxNumber,
                TaxCertificateImagePath = taxCertificateImagePath,
                BankAccountNumber = bankAccounNumber
            };

            return company;
        }
    }
}
