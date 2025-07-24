using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class EstablishMentRepresentitive
    {
        public EstablishMentRepresentitive()
        {
            this.Name = string.Empty;
            this.PhoneNumber = string.Empty;
            this.FrontIdentityNumberImagePath = string.Empty;
            this.BackIdentityNumberImagePath = string.Empty;
        }
        public int Id { get;private set; }
        public int EstablishmentId { get;private set; }
        public string Name { get;private set; }
        public string PhoneNumber { get; set; }
        public string FrontIdentityNumberImagePath { get;private set; }
        public string BackIdentityNumberImagePath { get;private set; }
        public EstablishMent EstablishMent { get;private set; }

        public static Result<EstablishMentRepresentitive>Instance(string name,
                                                                  string phoneNumber,
                                                                  string frontImage,
                                                                  string backImage)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<EstablishMentRepresentitive>("Name is Required");
            }
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Result.Failure<EstablishMentRepresentitive>("PhoneNumber is Required");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<EstablishMentRepresentitive>("Front IDentity Image is Required");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<EstablishMentRepresentitive>("Back Identity Image is Required");
            }

            return new EstablishMentRepresentitive
            {
                Name=name,
                PhoneNumber=phoneNumber,
                FrontIdentityNumberImagePath=frontImage,
                BackIdentityNumberImagePath=backImage
            };
        }
    }
}
