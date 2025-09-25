using Application.Features.CustomerSection.Feature.CustomerInfo.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.CustomerInfo.Queries
{
    public class GetCustomerInfoQueryHandler : IRequestHandler<GetCustomerInfoQuery, Result<CustomerInfoDto>>
    {
        private readonly INaqlahContext context;
        private readonly IUserSession userSession;

        public GetCustomerInfoQueryHandler(INaqlahContext context,
                                           IUserSession userSession)
        {
            this.context = context;
            this.userSession = userSession;
        }

        public async Task<Result<CustomerInfoDto>> Handle(GetCustomerInfoQuery request, CancellationToken cancellationToken)
        {
            var customer = await context.Customers
                .Include(c => c.Individual)
                .Include(c => c.EstablishMent)
                    .ThenInclude(e => e.EstablishMentRepresentitive)
                .Include(c => c.WalletTransctions)
                .FirstOrDefaultAsync(c => c.UserId == userSession.UserId, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<CustomerInfoDto>("Customer not found");
            }

            var customerInfo = new CustomerInfoDto
            {
                Id = customer.Id,
                PhoneNumber = customer.PhoneNumber,
                CustomerType = customer.CustomerType,
                WalletBalance = customer.WalletBalance
            };

            if (customer.CustomerType == Domain.Enums.CustomerType.Individual && customer.Individual != null)
            {
                customerInfo.Individual = new IndividualDto
                {
                    Id = customer.Individual.Id,
                    MobileNumber = customer.Individual.MobileNumber,
                    IdentityNumber = customer.Individual.IdentityNumber,
                    FrontIdentityImagePath = customer.Individual.FrontIdentityImagePath,
                    BackIdentityImagePath = customer.Individual.BackIdentityImagePath
                };
            }
            else if (customer.CustomerType == Domain.Enums.CustomerType.Establishment && customer.EstablishMent != null)
            {
                customerInfo.Establishment = new EstablishmentDto
                {
                    Id = customer.EstablishMent.Id,
                    Name = customer.EstablishMent.Name,
                    MobileNumber = customer.EstablishMent.MobileNumber,
                    RecordImagePath = customer.EstablishMent.RecoredImagePath,
                    TaxRegistrationNumber = customer.EstablishMent.TaxRegistrationNumber,
                    TaxRegistrationImagePath = customer.EstablishMent.TaxRegistrationImagePath,
                    Address = customer.EstablishMent.Address
                };

                if (customer.EstablishMent.EstablishMentRepresentitive != null)
                {
                    customerInfo.Establishment.Representative = new EstablishmentRepresentativeDto
                    {
                        Id = customer.EstablishMent.EstablishMentRepresentitive.Id,
                        Name = customer.EstablishMent.EstablishMentRepresentitive.Name,
                        PhoneNumber = customer.EstablishMent.EstablishMentRepresentitive.PhoneNumber,
                        FrontIdentityNumberImagePath = customer.EstablishMent.EstablishMentRepresentitive.FrontIdentityNumberImagePath,
                        BackIdentityNumberImagePath = customer.EstablishMent.EstablishMentRepresentitive.BackIdentityNumberImagePath
                    };
                }
            }

            return Result.Success(customerInfo);
        }
    }
}