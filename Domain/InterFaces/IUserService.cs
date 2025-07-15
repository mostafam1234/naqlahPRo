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
    }
}
