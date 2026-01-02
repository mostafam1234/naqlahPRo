using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Individual
    {
        public Individual()
        {
            this.MobileNumber = string.Empty;
            this.IdentityNumber = string.Empty;
            this.FrontIdentityImagePath = string.Empty;
            this.BackIdentityImagePath = string.Empty;
        }
        public int Id { get;private set; }
        public int CustomerId { get;private set; }
        public int Name { get;private set; }
        public string MobileNumber { get;private set; }
        public string IdentityNumber { get;private set; }
        public string FrontIdentityImagePath { get;private set; }
        public string BackIdentityImagePath { get;private set; }
        public Customer Customer { get;private set; }

        public static Result<Individual> Instance(string mobileNumber,
                                          string identityNumber,
                                          string frontIdentityImage,
                                          string backIdentityImage)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                return Result.Failure<Individual>("Mobile Number is Required");
            }

            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return Result.Failure<Individual>("Identity Number is Required");
            }

            //if (string.IsNullOrWhiteSpace(frontIdentityImage))
            //{
            //    return Result.Failure<Individual>("Identity Image is Required");
            //}

            //if (string.IsNullOrWhiteSpace(backIdentityImage))
            //{
            //    return Result.Failure<Individual>("Identity Image is Required");
            //}

            return new Individual
            {
                IdentityNumber = identityNumber,
                MobileNumber = mobileNumber,
                FrontIdentityImagePath = frontIdentityImage,
                BackIdentityImagePath = backIdentityImage
            };

        }
    }
}
