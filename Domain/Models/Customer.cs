using CSharpFunctionalExtensions;
using Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
            this._WalletTransctions = new List<WalletTransctions>();
        }
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string PhoneNumber { get; private set; }
        public CustomerType CustomerType { get; private set; }
        public Individual? Individual { get; private set; }
        public EstablishMent? EstablishMent { get; private set; }
        public string AndriodDevice { get; private set; }
        public string IosDevice { get; private set; }
        public User User { get; private set; }
        private List<WalletTransctions> _WalletTransctions { get; set; }
        public IReadOnlyList<WalletTransctions> WalletTransctions
        {
            get
            {
                return _WalletTransctions;
            }
            private set
            {
                _WalletTransctions = (List<WalletTransctions>)value.ToList();
            }
        }

        public static Result<Customer> InstanceAsIndividual(string phoneNumber,
                                                            string identtyNumber,
                                                            string frontIdentitImage,
                                                            string backIdentityImag)
        {
            var individual = Domain.Models.Individual.Instance(phoneNumber,
                                                               identtyNumber,
                                                               frontIdentitImage,
                                                               backIdentityImag);
            if (individual.IsFailure)
            {
                return Result.Failure<Customer>(individual.Error);
            }

            var customer = new Customer
            {
                PhoneNumber = phoneNumber,
                CustomerType = CustomerType.Individual,
                Individual = individual.Value
            };

            return customer;





        }


        public static Result<Customer> InstanceAsEstablishMent(string phoneNumber,
                                                               string name,
                                                               string recordImagePath,
                                                               string taxRegistrationNumber,
                                                               string taxRegestationImagePath,
                                                               string address,
                                                               string establishmentRepresentitveName,
                                                               string establishmentRepresentitveMobile,
                                                               string establishmentRepresentitvefrontImage,
                                                               string establishmentRepresentitveBackImage
                                                               )
        {

            var representitve = EstablishMentRepresentitive.Instance(establishmentRepresentitveName,
                                                                   establishmentRepresentitveMobile,
                                                                   establishmentRepresentitvefrontImage,
                                                                   establishmentRepresentitveBackImage);
            if (representitve.IsFailure)
            {
                return Result.Failure<Customer>(representitve.Error);
            }


            var establishment = EstablishMent.Instance(name,
                                                       phoneNumber,
                                                       recordImagePath,
                                                       taxRegistrationNumber,
                                                       taxRegestationImagePath,
                                                       address,
                                                       representitve.Value);

            if (establishment.IsFailure)
            {
                return Result.Failure<Customer>(establishment.Error);
            }

            var customer = new Customer
            {
                PhoneNumber = phoneNumber,
                CustomerType=CustomerType.Establishment,
                EstablishMent = establishment.Value
            };

            return customer;
        }

        public void AddFireBaseDevices(string androidDevice,string iosDevice)
        {
            if (!string.IsNullOrEmpty(androidDevice))
            {
                this.AndriodDevice = androidDevice;
            }

            if (!string.IsNullOrEmpty(iosDevice))
            {
                this.AndriodDevice = iosDevice;
            }

        }
    }
}
