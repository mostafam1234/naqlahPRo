using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Assistant
    {
        public Assistant()
        {
            this.PhoneNumber = string.Empty;
            this.IdentityNumber = string.Empty;
            this.FrontIdentityImagePath = string.Empty;
            this.BackIdentityImagePath = string.Empty;
            this.Name = string.Empty;
            this.Address = string.Empty;
        }
        public int Id { get; private set; }
        public int AssistanWorkId { get; private set; }
        public string PhoneNumber { get; private set; }
        public string IdentityNumber { get; private set; }
        public string Name { get;private set; }
        public string Address { get;private set; }
        public int? DeliveryManId { get; private set; }
        public string FrontIdentityImagePath { get; private set; }
        public string BackIdentityImagePath { get; private set; }
        public DateTime IdentityExpirationDate { get; private set; }
        public AssistanWork AssistanWork { get; private set; }
        public DeliveryMan? DeliveryMan { get; private set; }
       

        public static Result<Assistant>Instance(string name,
                                                string address,
                                                string phoneNumber,
                                                string identityNumber,
                                                string frontImage,
                                                string backImage,
                                                DateTime identityExpirationDate,
                                                int assistantWorkId,
                                                int? delivryManId)
        {
            var assistant = new Assistant
            {
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                IdentityNumber = identityNumber,
                FrontIdentityImagePath = frontImage,
                BackIdentityImagePath = backImage,
                IdentityExpirationDate = identityExpirationDate,
                AssistanWorkId = assistantWorkId,
                DeliveryManId = delivryManId
            };

            return assistant;
        }
    }
}
