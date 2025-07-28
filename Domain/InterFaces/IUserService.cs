using CSharpFunctionalExtensions;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InterFaces
{
    public interface IUserService
    {
        Task<Result<int>> CreateDeliveryUser(string mobile,
                                             string email,
                                             string name,
                                             string password);

        Task<Result<TokenResponse>> GetAcessToken(string userName,
                                                  string Password);

        Task<Result> CheckUserPassword(string userName,
                                       string password);

        Task<Result<int>> CreateCustomerUserAsEstablishMent(string phoneNumber,
                                                            string name,
                                                            string recordImagePath,
                                                            string taxRegistrationNumber,
                                                            string taxRegestationImagePath,
                                                            string address,
                                                            string establishmentRepresentitveName,
                                                            string establishmentRepresentitveMobile,
                                                            string establishmentRepresentitvefrontImage,
                                                            string establishmentRepresentitveBackImage,
                                                            string password);
        Task<Result<int>> CreateCustomerUserAsIndividual(string phoneNumber,
                                                         string identtyNumber,
                                                         string frontIdentitImage,
                                                         string backIdentityImag,
                                                         string password);
    }
}
