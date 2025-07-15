using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Regestration.Dtos
{
    public class CompanyRequest
    {
        public CompanyRequest()
        {
            this.CompanyName = string.Empty;
            this.TaxNumber = string.Empty;
            this.CommercialRecordNumber = string.Empty;
            this.RecordImagePath = string.Empty;
            this.BankAccountNumber = string.Empty;
            this.TaxCertificateImage = string.Empty;
        }
        public string CompanyName { get;  set; }
        public string CommercialRecordNumber { get;  set; }
        public string RecordImagePath { get;  set; }
        public string TaxNumber { get;  set; }
        public string TaxCertificateImage { get;  set; }
        public string BankAccountNumber { get; set; }
    }
}
